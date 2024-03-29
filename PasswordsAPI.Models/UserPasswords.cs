﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Passwords.API.Abstracts;

namespace Passwords.API.Models
{
    public class UserPasswords : EntityBase<UserPasswords>, IEntityBase
    {
        public new static readonly UserPasswords Invalid = 
            new (new Status(ResultCode.Invalid|ResultCode.Password));

        Status IEntityBase.DefaultStatus { get { return new Status(ResultCode.Password); } }

        //------------------------------//

        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("PasswordUsers")]
        public int User { get; set; }
        [Required]
        public ulong Hash { get; set; }
        [MaybeNull]
        public string Pass { get; set; }

        //------------------------------//

        public UserPasswords( Status invalid )
            : base( invalid ) {
            Is().Status = invalid.Code > 0 
                  ? invalid : Invalid.Is().Status;
        }

        public UserPasswords() : base()
        {
            Hash = 0;
            Pass = String.Empty;
            Id = 0;
            User = 0;
        }

        public static implicit operator bool( UserPasswords cast )
        {
            return cast.Is().Status;
        }

        public static implicit operator UserPasswords( Status cast )
        {
            return new UserPasswords( cast );
        }

    }
}
