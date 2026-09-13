using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Domain.Enums
{
    /// <summary>
    /// Predefined system and operational roles for Role-Based Access Control (RBAC).
    /// </summary>
    public enum ApplicationRoleType
    {
        [Display(Name = "Super Admin", Description = "Full unrestricted access to all administrative modules, security controls, and store configurations.")]
        SuperAdmin = 1,

        [Display(Name = "Admin", Description = "Full management access to store operations, catalogs, sales, customers, and CMS content.")]
        Admin = 2,

        [Display(Name = "Order Manager", Description = "Access to manage customer orders, order fulfillment status, invoices, and live shopping carts.")]
        OrderManager = 3,

        [Display(Name = "Catalog Manager", Description = "Access to manage product catalogs, categories, brands, inventory stocks, coupons, and flash deals.")]
        CatalogManager = 4,

        [Display(Name = "Blog Editor", Description = "Access to author, publish, and edit blog articles and moderate reader comments.")]
        BlogEditor = 5,

        [Display(Name = "Support Staff", Description = "Access to customer inquiries, support messages, newsletter subscribers, and product reviews.")]
        SupportStaff = 6,

        [Display(Name = "Customer", Description = "Standard storefront shopper account with access to browse, purchase, and review orders.")]
        Customer = 7
    }

    public static class ApplicationRoleTypeExtensions
    {
        public static string GetDescription(this ApplicationRoleType role)
        {
            return role switch
            {
                ApplicationRoleType.SuperAdmin => "Full unrestricted access to all administrative modules, security controls, and store configurations.",
                ApplicationRoleType.Admin => "Full management access to store operations, catalogs, sales, customers, and CMS content.",
                ApplicationRoleType.OrderManager => "Access to manage customer orders, order fulfillment status, invoices, and live shopping carts.",
                ApplicationRoleType.CatalogManager => "Access to manage product catalogs, categories, brands, inventory stocks, coupons, and flash deals.",
                ApplicationRoleType.BlogEditor => "Access to author, publish, and edit blog articles and moderate reader comments.",
                ApplicationRoleType.SupportStaff => "Access to customer inquiries, support messages, newsletter subscribers, and product reviews.",
                ApplicationRoleType.Customer => "Standard storefront shopper account with access to browse, purchase, and review orders.",
                _ => string.Empty
            };
        }

        public static string GetDisplayName(this ApplicationRoleType role)
        {
            return role switch
            {
                ApplicationRoleType.SuperAdmin => "Super Admin",
                ApplicationRoleType.Admin => "Admin",
                ApplicationRoleType.OrderManager => "Order Manager",
                ApplicationRoleType.CatalogManager => "Catalog Manager",
                ApplicationRoleType.BlogEditor => "Blog Editor",
                ApplicationRoleType.SupportStaff => "Support Staff",
                ApplicationRoleType.Customer => "Customer",
                _ => role.ToString()
            };
        }
    }
}

