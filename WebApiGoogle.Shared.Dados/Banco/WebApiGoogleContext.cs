using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiGoogle.Shared.Dados.Models;

namespace WebApiGoogle.Shared.Dados.Banco;

public class WebApiGoogleContext : IdentityDbContext<ApplicationUser, AccessProfile, int>
{

    public WebApiGoogleContext()
    {

    }

    public WebApiGoogleContext(DbContextOptions options)
       : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }
        optionsBuilder
            .UseLazyLoadingProxies();
    }


}
