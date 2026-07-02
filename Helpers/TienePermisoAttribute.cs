using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiAegis.Helpers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class TienePermisoAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permiso;

        public TienePermisoAttribute(string permiso)
        {
            _permiso = permiso;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Verificar si el usuario posee la claim 'permiso' con el valor correspondiente
            var tienePermiso = user.Claims
                .Any(c => c.Type == "permiso" && c.Value == _permiso);

            if (!tienePermiso)
            {
                // Si es Administrador, tiene acceso completo por defecto
                var esAdmin = user.IsInRole("Administrador");
                if (!esAdmin)
                {
                    context.Result = new JsonResult(new { mensaje = "No tienes permiso para realizar esta acción" })
                    {
                        StatusCode = 403
                    };
                }
            }
        }
    }
}
