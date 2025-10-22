using System.Runtime.InteropServices;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureBase.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        // await initialiser.SeedAsync();

        await Task.CompletedTask;
    }
}

//Vai trò: Khởi tạo và nạp dữ liệu cho cơ sở dữ liệu
public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UserAccount> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUser _user;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context,
        UserManager<UserAccount> userManager, RoleManager<ApplicationRole> roleManager, IUser user)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _user = user;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            //OPTION: Xóa database hiện tại
            // await _context.Database.EnsureDeletedAsync();
            //Thực hiện các migrations chưa được áp dụng
            // var databaseExists = await _context.Database.EnsureCreatedAsync();

            // if (!databaseExists)
            // {
            await _context.Database.MigrateAsync();
            // }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            // await TrySeedAsync();
            await Task.CompletedTask;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        Guid adminId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        if (!_context.Users.Any())
        {
            // Default roles
            var administratorRole = new ApplicationRole(Roles.Administrator);
            var moderatorRole = new ApplicationRole(Roles.Moderator);
            var userRole = new ApplicationRole(Roles.User);

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }

            if (_roleManager.Roles.All(r => r.Name != moderatorRole.Name))
            {
                await _roleManager.CreateAsync(moderatorRole);
            }

            if (_roleManager.Roles.All(r => r.Name != userRole.Name))
            {
                await _roleManager.CreateAsync(userRole);
            }

            var users = _userManager.Users.ToList();

            // Default users
            var admin = new User
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                FullName = "Admin",
                Email = "sa@gmail.com",
                IsBanned = false
            };
            var administrator = new UserAccount
            {
                Id = admin.Id,
                UserName = "77777777-7777-7777-7777-777777777777",
                Email = "sa@gmail.com",
                IsDeleted = false,
                User = admin,
                EmailConfirmed = true
            };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Sa@1234");
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, new[]
                    {
                        administratorRole.Name
                    });
                }
            }


            var moderatorUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Moderator",
                Email = "moderator@gmail.com",
                IsBanned = false
            };
            var moderatorAccount = new UserAccount
            {
                Id = moderatorUser.Id,
                UserName = moderatorUser.Id.ToString(),
                Email = "moderator@gmail.com",
                IsDeleted = false,
                User = moderatorUser,
                EmailConfirmed = true
            };

            if (_userManager.Users.All(u => u.UserName != moderatorAccount.UserName))
            {
                await _userManager.CreateAsync(moderatorAccount, "123456");
                await _userManager.AddToRoleAsync(moderatorAccount, Roles.Moderator);
            }
        }


        if (!_context.Categories.Any())
        {
            var menCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Men",
                IsDeleted = false,
                CreatedBy = adminId
            };
            var womenCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Women",
                IsDeleted = false,
                CreatedBy = adminId
            };
            var tshirtCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "T-Shirts",
                ParentCategory = menCategory,
                IsDeleted = false,
                CreatedBy = adminId
            };

            _context.Categories.AddRange(menCategory, womenCategory, tshirtCategory);
            await _context.SaveChangesAsync();
        }


        if (!_context.Products.Any())
        {
            var menCategory = _context.Categories.First(c => c.Name == "Men");
            var tshirtCategory = _context.Categories.First(c => c.Name == "T-Shirts");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Basic Black T-Shirt",
                Description = "Comfortable cotton t-shirt",
                BasePrice = 150000,
                ImageUrl = "https://example.com/tshirt-black.jpg",
                IsDeleted = false,
                ProductCategories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        CategoryId = menCategory.Id,
                    },
                    new ProductCategory
                    {
                        CategoryId = tshirtCategory.Id
                    }
                },
                CreatedBy = adminId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // ==== Product Options ====
            var sizeOption = new ProductOption
            {
                Id = Guid.NewGuid(),
                Name = "Size",
                ProductId = product.Id,
                Values = new List<ProductOptionValue>
                {
                    new ProductOptionValue
                    {
                        Id = Guid.NewGuid(),
                        Value = "S"
                    },
                    new ProductOptionValue
                    {
                        Id = Guid.NewGuid(),
                        Value = "M"
                    },
                    new ProductOptionValue
                    {
                        Id = Guid.NewGuid(),
                        Value = "L"
                    },
                }
            };

            var colorOption = new ProductOption
            {
                Id = Guid.NewGuid(),
                Name = "Color",
                ProductId = product.Id,
                Values = new List<ProductOptionValue>
                {
                    new ProductOptionValue
                    {
                        Id = Guid.NewGuid(),
                        Value = "Black"
                    },
                    new ProductOptionValue
                    {
                        Id = Guid.NewGuid(),
                        Value = "White"
                    }
                }
            };

            _context.ProductOptions.AddRange(sizeOption, colorOption);
            await _context.SaveChangesAsync();

            // ==== Product Variants ====
            var blackM = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                UnitPrice = 150000,
                Stock = 50,
                Sku = "TSHIRT-BLK-M",
            };

            var whiteL = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                UnitPrice = 160000,
                Stock = 30,
                Sku = "TSHIRT-WHT-L",
            };

            _context.ProductVariants.AddRange(blackM, whiteL);
            await _context.SaveChangesAsync();
        }
    }
}
