﻿using System;
using Xunit;
using PasswordsAPI.Abstracts;
using PasswordsAPI.Tests.Helpers;
using Yps;


namespace PasswordsAPI.Services.Tests
{
    public class UserPasswordsServiceTests : IDisposable
    {
        [Fact]
        private void UserPasswordsService_CreatingMasterPassword()
        { 
            // Arrange
            Test.Context.PrepareDataBase("UserPasswordsService","OneUserNoPassword");
            PasswordUsersService<Test.Context> usinger = new PasswordUsersService<Test.Context>(Test.CurrentContext);
            UserPasswordsService<Test.Context> service = new UserPasswordsService<Test.Context>(Test.CurrentContext, usinger);

            // Act
            Status result = service.SetMasterKey(1,"ElMaestro").GetAwaiter().GetResult().Status;

            // Assert
            Assert.True(result.Text.Length>0, result);
        }

        [Fact]
        private void UserPasswordsService_ChangingMasterPassword()
        {
            // Arrange
            Test.Context.PrepareDataBase("UserPasswordsService", "OneValidUserAccount");
            PasswordUsersService<Test.Context> usinger = new PasswordUsersService<Test.Context>(Test.CurrentContext);
            UserPasswordsService<Test.Context> service = new UserPasswordsService<Test.Context>(Test.CurrentContext, usinger);

            // Act
            Status result = service.SetMasterKey(1, "ElBongo").GetAwaiter().GetResult().Status;
            bool valid = service.VerifyPassword(1, "ElBongo");

            // Assert
            Assert.True( valid, result );
        }

        [Fact]
        private void UserPasswordsService_PasswordVerification()
        {
            // Arrange
            Test.Context.PrepareDataBase("UserPasswordsService","OneValidUserAccount");
            PasswordUsersService<Test.Context> usinger = new PasswordUsersService<Test.Context>(Test.CurrentContext);
            UserPasswordsService<Test.Context> service = new UserPasswordsService<Test.Context>(Test.CurrentContext, usinger);

            // Act
            bool valid = service.VerifyPassword(1, "ElMaestro");

            // Assert
            Assert.True( valid , service.Status );
        }

        [Fact]
        private void UserPasswordsService_GetMasterKeyWorks()
        {
            // Arrange
            Test.Context.PrepareDataBase("UserPasswordsService","OneValidUserAccount");
            PasswordUsersService<Test.Context> usinger = new PasswordUsersService<Test.Context>(Test.CurrentContext);
            UserPasswordsService<Test.Context> service = new UserPasswordsService<Test.Context>(Test.CurrentContext, usinger);

            // Act
            CryptKey key = service.GetMasterKey(1);

            // Assert
            Assert.True( service, service.Status.ToString() );
            Assert.True( key.IsValid(), service.Status );
            Assert.True( key.VerifyPhrase( "ElMaestro" ) );
        }

        [Fact]
        private void UserPasswordsService_ErroOnFailedPasswordVerification()
        {
            // Arrange
            Test.Context.PrepareDataBase("UserPasswordsService", "OneValidUserAccount");
            PasswordUsersService<Test.Context> usinger = new PasswordUsersService<Test.Context>(Test.CurrentContext);
            UserPasswordsService<Test.Context> service = new UserPasswordsService<Test.Context>(Test.CurrentContext, usinger);

            // Act
            bool valid = service.VerifyPassword( 1, "ElPisso" );

            // Assert
            Assert.False( valid, service.Status );
            Assert.False( service, service.Status );
        }

        [Fact]
        private void UserPasswordsService_ErroOnInvalidUsersPasswordVerification()
        {
            // Arrange
            Test.Context.PrepareDataBase("UserPasswordsService", "OneValidUserAccount");
            PasswordUsersService<Test.Context> usinger = new PasswordUsersService<Test.Context>(Test.CurrentContext);
            UserPasswordsService<Test.Context> service = new UserPasswordsService<Test.Context>(Test.CurrentContext, usinger);

            // Act
            CryptKey key = service.GetMasterKey(1000);

            // Assert
            Assert.False( service, service.Status );
            Assert.False( key.IsValid(), service.Status );
        }

        public void Dispose()
        {
            Test.CurrentContext.Finished();
        }
    }
}