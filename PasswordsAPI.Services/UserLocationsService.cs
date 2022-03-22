﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordsAPI.Abstracts;
using PasswordsAPI.Models;
using Yps;

namespace PasswordsAPI.Services
{
    public class UserLocationsService<CTX>
        : AbstractApiService<UserLocations,UserLocationsService<CTX>,CTX>
        where CTX : PasswordsApiDbContext<CTX>
    {
        private static readonly Status LocationServiceError = new Status(
                                    ResultCode.Area | ResultCode.Service |
                                    ResultCode.Invalid,"Invalid Location" );

        protected override Status GetDefaultError() {
            return LocationServiceError;
        }

        private Crypt.Key?                 _key = null;
        private UserPasswordsService<CTX>  _keys;


        public override UserLocations Entity {
            get {
                if (_enty.Is().Status.Waiting)
                    _enty = _lazy.GetAwaiter().GetResult() 
                        ?? LocationServiceError;
                return Ok ? _enty : Status;
            }
            set {
                if ( value ) { _enty = value;
                    Status = _enty.Is().Status; }
                else Status = value.Is().Status;
            }
        }

        public bool Update()
        {
            if( Entity.Is().Status.Ok ) {
                _db.Update( Entity );
                _db.SaveChanges();
                return true;
            } return false;
        }

        public List<UserLocations> GetUserLocations(int user)
        {
            Func<UserLocations, UserLocations> selector = (UserLocations u) => { return u.User == user ? u : null; };
            IEnumerator<UserLocations> locations = _dset.AsNoTracking().Select(selector).GetEnumerator();
            List<UserLocations> returnList = new List<UserLocations>();
            while ( locations.MoveNext() ) {
                if ( locations.Current != null ) {
                    returnList.Add( locations.Current );
                }
            } locations.Dispose();
            return returnList;
        }

        public UserLocationsService(CTX ctx, IPasswordsApiService<UserPasswords,UserPasswordsService<CTX>,CTX> pwd )
            : base( ctx )
        {
            _keys = pwd.serve();
            _enty = UserLocations.Invalid;
            _lazy = new Task<UserLocations>(() => { return _enty; });
        }

        public async Task<UserLocationsService<CTX>> SetKey( string masterPass )
        {
            if( Entity.Is().Status.Bad ) return this; 
            if( _keys.VerifyPassword( Entity.User, masterPass ) ) _key = _keys.GetMasterKey( Entity.User );
            return _keys.Status 
                 ? OnError( _keys )
                 : this;
        }

        public async Task<UserLocationsService<CTX>> SetKey( Crypt.Key masterKey )
        {
            if( !masterKey.IsValid() ) {
                Status = (LocationServiceError + ResultCode.Cryptic).WithText( "Invalid Crypt.Key" );
                _key = null;
            } else {
                Ok = true;
                _key = masterKey;
            } return this;
        }

        public string GetPassword()
        {
            string cryptic = Encoding.Default.GetString( Entity?.Pass ?? new byte[]{} );
            return _key?.Decrypt( cryptic ) ?? cryptic;
        }

        public string GetPassword( string masterPass )
        {
            string crypt = SetKey( Crypt.CreateKey( masterPass ) ).GetAwaiter().GetResult().GetPassword();
            if ( Crypt.Error ) {
                Status = new Status( Crypt.Error.Code.ToError(), Crypt.Error.Text, "still encrypted: " + crypt );
            } return crypt;
        }

        public async Task<UserLocationsService<CTX>> SetPassword( string userMasterPass, string newLocationPass )
        {
            if ( Entity.Id > 0 ) {
                if ( (await _keys.ByUserId( Entity.User )).VerifyPassword( Entity.User, userMasterPass ) ) {
                    Crypt.Key key = _keys.GetMasterKey( Entity.User );
                    Entity.Pass = Encoding.ASCII.GetBytes( key.Encrypt( newLocationPass ) );
                    _db.Update( Entity );
                    _db.SaveChanges();
                } else Status = _keys.Status;
                return this;
            } else {
                Status = LocationServiceError.WithText( "unknown user id" );
                return this;
            }
        }

        public int GetAreaId( string nameOrId, int usrId )
        {
            Status = Status.NoError;
            if ( int.TryParse( nameOrId, out int locId ) ) {
                if ( Entity.IsValid() )
                    if ( Entity.User == usrId && Entity.Id == locId )
                        return locId;
                _enty = Status.Unknown;
                _lazy = _dset.AsNoTracking().SingleOrDefaultAsync(l => l.User == usrId && l.Id == locId);
            } else {
                if ( Entity.IsValid() )
                    if ( Entity.User == usrId && Entity.Area == nameOrId )
                        return Entity.Id;
                _enty = Status.Unknown;
                _lazy = _dset.AsNoTracking().SingleOrDefaultAsync(l => l.User == usrId && l.Area == nameOrId);
            }
            if ( Entity ) return Entity.Id;
            else return -1;
        }

        public async Task<UserLocationsService<CTX>> GetLocationEntity( int userId, string location )
        {
            if ( userId > 0 ) {
                if ( GetAreaId( location, userId ) == -1 ) Status = LocationServiceError.WithText( $"invalid location '{location}'" );
            } else {
                _enty = Status = LocationServiceError.WithText( $"invalid user '{userId}'" ) + ResultCode.User;
            } return this;
        }

        public async Task<UserLocationsService<CTX>> GetLocationEntity( int locationId )
        {
            if ( locationId > 0 ) {
                Status = Status.NoError;
                if (Entity) if (Entity.Id == locationId) return this;
                _lazy = _dset.AsNoTracking().SingleOrDefaultAsync(l => l.Id == locationId);
                _enty = Status.Unknown;
            } else _enty= Status = LocationServiceError.WithText( $"invalid location id '{locationId}'" );
            return this;
        }

        public UserLocationsService<CTX> AddNewLocationEntry( UserLocations init, string passToStore, Crypt.Key masterKey )
        {
            if ( Status.Bad ) return this;
            if ( !masterKey.IsValid() ) {
                _enty= Status = new Status( LocationServiceError.Code | ResultCode.Cryptic | ResultCode.Password, "Invalid Master Key" );
                return this;
            } _enty = init;

            _enty.Pass = Encoding.ASCII.GetBytes( masterKey.Encrypt( passToStore ) );
            if( Crypt.Error ) {
                _enty = Status = new Status( LocationServiceError.Code|Status.Cryptic.Code, Crypt.Error.ToString(), passToStore );
                return this;
            }
            // this should be moved to a repository class
            _dset.AddAsync( _enty );
            _db.SaveChangesAsync();
            return this;
        }

        public async Task<UserLocationsService<CTX>> SetLocationPassword( Task<PasswordUsersService<CTX>> usrserv, UserLocations init, string pass )
        {
            PasswordUsers usr = (await usrserv).Entity;
            if( usr.Is().Status.Bad ) {
                Status = new Status( (
                        ResultCode.Area | ResultCode.Service |
                        ResultCode.User | ResultCode.Invalid )
                    ,"Unknown User"
                );
                return this;
            }

            if ( ! await _keys.ForUserAccount( usrserv ) )
                return OnError( _keys );

            Crypt.Key masterKey = _keys.GetMasterKey( usr.Id );
            if ( await GetLocationEntity( init.User = usr.Id, init.Area ) ) {
                // if the location already exists, update with new password set
                _enty.Pass = Encoding.ASCII.GetBytes( masterKey.Encrypt( pass ) );
                _dset.Update( _enty );
                Status = Status.NoError;
                _db.SaveChangesAsync();
                return this;
            } else {
                // if location not exists yet, add a new location entry therefore
                _enty = Status = Status.NoError;
                return AddNewLocationEntry( init, pass, masterKey );
            }
        }

        public async Task<UserLocationsService<CTX>> SetLoginInfo( int locId, string? login, string? info )
        {
            if( (await GetLocationEntity( locId )).Entity.Is().Status.Ok ) {
                if (info != null) if (info != String.Empty) _enty.Info = info;
                if (login != null) if (login != String.Empty) _enty.Name = login;
                _dset.Update(_enty);
                _db.SaveChangesAsync();
            } return this;
        }

        public override string ToString()
        {
            if ( Status.Bad || Entity.Is().Status.Bad ) return Status.ToString();
            StringBuilder str = new StringBuilder("{ Id:");
            str.Append( _enty.Id ).Append( ", User:" ).Append( _enty.User ).Append( ", Name:'" ).Append( _enty.Area );
            if ( _enty.Info != null ) str.Append( "', Info:'" ).Append( _enty.Info );
            if ( _enty.Name != null ) str.Append( "', LoginName:'" ).Append( _enty.Name );
            str.Append( "', Password:'" ).Append( GetPassword() ).Append( "' }" );
            return str.ToString();
        }

        public async Task<UserLocationsService<CTX>> RemoveLocation( Task<PasswordUsersService<CTX>> userserv, string area, string master )
        {
            PasswordUsers user = (await userserv).Entity;
            UserPasswords password = ( await _keys.ForUserAccount( userserv ) ).Entity;
            if ( password.Is().Status.Ok ) {
                if( _keys.VerifyPassword( user.Id, master ) ) {
                    if ( await GetLocationEntity( user.Id, area ) ) {
                        _dset.Remove( Entity );
                        _db.SaveChanges();
                    } else Status = LocationServiceError.WithText( "Location '{0}' not exists" ).WithData( area );
                } else Status = LocationServiceError.WithData( master ).WithText( 
                  "For Deleting a Passwords, the owning users master password is needed"
                ) + ( ResultCode.Invalid | ResultCode.User | ResultCode.Password );
            } return this;
        }
    }
}
