using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CarRentalHajurKo.Data;
using CarRentalHajurKo.Models;

public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarRentalHajurKo.Models.Car> Car { get; set; } = default!;
        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<CarRentalHajurKo.Models.Staff>? Staff { get; set; }
        public DbSet<CarRentalHajurKo.Models.Order>? Orders { get; set; }
        public DbSet<CarRentalHajurKo.Models.DamageRequest>? DamageRequest { get; set; }
        public DbSet<CarRentalHajurKo.Models.DamageReport>? DamageReport { get; set; }
    }
