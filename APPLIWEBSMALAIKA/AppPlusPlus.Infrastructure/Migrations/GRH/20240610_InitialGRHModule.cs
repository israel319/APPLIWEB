using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppPlusPlus.Infrastructure.Migrations.GRH
{
    /// <inheritdoc />
    public partial class InitialGRHModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========== ORGANIZATION TABLES ==========
            migrationBuilder.CreateTable(
                name: "T_GRH_Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    DefaultCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "TND"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GRH_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_GRH_EmployeeCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HealthInsuranceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PensionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FoodAllowance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TransportAllowance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    HousingAllowance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ChildAllowance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    LeaveEntitlement = table.Column<int>(type: "int", nullable: true),
                    TrainingBudget = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GRH_EmployeeCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_GRH_EmployeeClassification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassificationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClassificationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasSocialSecurity = table.Column<bool>(type: "bit", nullable: false),
                    HasBenefits = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GRH_EmployeeClassification", x => x.Id);
                });

            // Créer les autres tables (mise en place simple pour la migration)
            // Toutes les tables ont déjà été créées par le script SQL utilisateur

            // Créer les indexes
            migrationBuilder.CreateIndex(
                name: "IX_T_GRH_Company_CompanyName",
                table: "T_GRH_Company",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_T_GRH_EmployeeCategory_CategoryCode",
                table: "T_GRH_EmployeeCategory",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_GRH_EmployeeClassification_ClassificationCode",
                table: "T_GRH_EmployeeClassification",
                column: "ClassificationCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Suppression des indexes
            migrationBuilder.DropIndex(
                name: "IX_T_GRH_Company_CompanyName",
                table: "T_GRH_Company");

            migrationBuilder.DropIndex(
                name: "IX_T_GRH_EmployeeCategory_CategoryCode",
                table: "T_GRH_EmployeeCategory");

            migrationBuilder.DropIndex(
                name: "IX_T_GRH_EmployeeClassification_ClassificationCode",
                table: "T_GRH_EmployeeClassification");

            // Suppression des tables
            migrationBuilder.DropTable(
                name: "T_GRH_Company");

            migrationBuilder.DropTable(
                name: "T_GRH_EmployeeCategory");

            migrationBuilder.DropTable(
                name: "T_GRH_EmployeeClassification");
        }
    }
}
