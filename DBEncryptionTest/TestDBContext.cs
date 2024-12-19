using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DBEncryptionTest
{
    public class TestDBContext : DbContext
    {
        public TestDBContext(DbContextOptions<TestDBContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
    }
}
