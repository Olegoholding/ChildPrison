using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChildPrison.Models
{

    [Table("employee_statuses")]
    public class EmployeeStatus
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("status_name")]
        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; }

        [Column("description")]
        [MaxLength(200)]
        public string Description { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    [Table("employees")]
    public class Employee
    {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column("mid_name")]
        [Required]
        [MaxLength(100)]
        public string MidName { get; set; }

        [Column("last_name")]
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column("profession")]
        [MaxLength(100)]
        public string Profession { get; set; }

        [Column("education")]
        public string Education { get; set; }

        [Column("status_id")]
        public int? StatusId { get; set; }

        [Column("hire_date")]
        public DateTime? HireDate { get; set; }

        [ForeignKey("StatusId")]
        public virtual EmployeeStatus Status { get; set; }

        [NotMapped]
        public string FullName
        {
            get => $"{LastName} {FirstName} {MidName}".Trim();
            set { } 
        }
    }

    [Table("children")]
    public class Child
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column("mid_name")]
        [MaxLength(100)]
        public string MidName { get; set; }

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("parent_id")]
        public int ParentId { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MidName}".Trim();
    }

    [Table("parents")]
    public class Parent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("last_name")]
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column("first_name")]
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column("middle_name")]
        [MaxLength(100)]
        public string MiddleName { get; set; }

        [Column("contact_info")]
        public string ContactInfo { get; set; }

        public virtual ICollection<Child> Children { get; set; } = new List<Child>();
    }


    [Table("groups")]
    public class Group
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("group_name")]
        [Required]
        [MaxLength(50)]
        public string GroupName { get; set; }

        [Column("teacher_id")]
        public int? TeacherId { get; set; }

        [Column("room_number")]
        [MaxLength(20)]
        public string RoomNumber { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Employee Teacher { get; set; }

        public virtual ICollection<Child> Children { get; set; } = new List<Child>();

        [NotMapped]
        public string TeacherFullName
        {
            get
            {
                if (Teacher != null)
                {
                    return $"{Teacher.LastName} {Teacher.FirstName} {Teacher.MidName}".Trim();
                }
                return "НЕ НАЗНАЧЕН";
            }
        }

        [NotMapped]
        public int ChildrenCount => Children?.Count ?? 0;
    }

    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_name")]
        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        //public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }

    [Table("suppliers")]
    public class Supplier
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("contact_info")]
        public string ContactInfo { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    [Table("dish_products")]
    public class DishProduct
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("dish_id")]
        public int DishId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public double? Quantity { get; set; }

        [Column("unit")]
        [MaxLength(20)]
        public string Unit { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    [Table("dishes")]
    public class Dish
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("dish_name")]
        [Required]
        [MaxLength(255)]
        public string DishName { get; set; }

        public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

        public virtual ICollection<DishProduct> DishProducts { get; set; } = new List<DishProduct>();

        [NotMapped]
        public int MenusCount => Menus?.Count ?? 0;

        [NotMapped]
        public string IngredientsList
        {
            get
            {
                if (DishProducts == null || !DishProducts.Any())
                    return "НЕТ ИНГРЕДИЕНТОВ";
                return string.Join(", ", DishProducts.Select(dp =>
                    $"{dp.Product?.ProductName} ({dp.Quantity} {dp.Unit})"));
            }
        }
    }

    [Table("menu")]
    public class Menu
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("dish_id")]
        public int? DishId { get; set; }

        [Column("meal_time")]
        [MaxLength(50)]
        public string MealTime { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }
    }

    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("login")]
        [Required]
        [MaxLength(100)]
        public string Login { get; set; }

        [Column("password_hash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Column("first_name")]
        [Required]
        [MaxLength(45)]
        public string FirstName { get; set; }

        [Column("mid_name")]
        [Required]
        [MaxLength(45)]
        public string MidName { get; set; }

        [Column("last_name")]
        [Required]
        [MaxLength(45)]
        public string LastName { get; set; }
    }
}