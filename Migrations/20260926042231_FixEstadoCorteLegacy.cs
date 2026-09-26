using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiAegis.Migrations
{
    /// <inheritdoc />
    public partial class FixEstadoCorteLegacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normaliza los cortes antiguos que no tenían estado ni tipo de corte
            migrationBuilder.Sql(@"UPDATE `caja_cierres` SET `estado_corte` = CASE WHEN `diferencia` < 0 THEN 'FALTANTE' WHEN `diferencia` > 0 THEN 'SOBRANTE' ELSE 'CUADRADO' END WHERE `estado_corte` = '' OR `estado_corte` IS NULL;");
            migrationBuilder.Sql(@"UPDATE `caja_cierres` SET `tipo_corte` = 'NORMAL' WHERE `tipo_corte` = '' OR `tipo_corte` IS NULL;");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$.VtsimgEsCaKe3sy1AG6HuF9rRe1nm0JepyT4CJIMtkmMf662rJBC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$1VGFXpJVcst493X3Ijvfd.GFTYXRNKU/GkLOxSkoauANMvhtM3blC");
        }
    }
}
