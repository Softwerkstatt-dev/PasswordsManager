﻿using Microsoft.EntityFrameworkCore;
using Passwords.API.Abstracts;
using Passwords.API.Models;


namespace Passwords.API.Database
{
    public class PasswordsDbContext
        : PasswordsApiDbContext<PasswordsDbContext>
    {
        public PasswordsDbContext( DbContextOptions<PasswordsDbContext> options )
            : base(options)
        { }

        public virtual DbSet<PasswordUsers> PasswordUsers
        {
            get { return EntitiesSet<PasswordUsers>(); }
            set { SetEntities( value ); }
        }
        public virtual DbSet<UserPasswords> UserPasswords
        {
            get { return EntitiesSet<UserPasswords>(); }
            set { SetEntities( value ); }
        }
        public virtual DbSet<UserLocations> UserLocations
        {
            get { return EntitiesSet<UserLocations>(); }
            set { SetEntities( value ); }
        }
    }
}
