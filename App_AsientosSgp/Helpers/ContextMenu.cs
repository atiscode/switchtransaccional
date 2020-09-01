using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Helpers
{
    public static class ContextMenu
    {
        public static int GetSumOptions(string optionsNames)
        {
            int ret = 0;
            if (optionsNames.Contains("Eliminar")) ret += 1;
            if (optionsNames.Contains("Contactos")) ret += 2;
            if (optionsNames.Contains("Regimen")) ret += 4;
            if (optionsNames.Contains("Editar")) ret += 8;
            if (optionsNames.Contains("Ayuda")) ret += 16;
            if (optionsNames.Contains("Tarifas")) ret += 32;
            if (optionsNames.Contains("Alojamiento")) ret += 64;
            if (optionsNames.Contains("Precios")) ret += 128;
            if (optionsNames.Contains("Cupos")) ret += 256;
            if (optionsNames.Contains("Validar")) ret += 512;
            if (optionsNames.Contains("Resumen")) ret += 1024;
            if (optionsNames.Contains("Imagenes")) ret += 2048;
            if (optionsNames.Contains("Ciudades")) ret += 4096;
            if (optionsNames.Contains("Sectores")) ret += 8192;
            if (optionsNames.Contains("Productos")) ret += 16384;
            if (optionsNames.Contains("ImagenHab")) ret += 32768;
            if (optionsNames.Contains("Markup")) ret += 65536;
            if (optionsNames.Contains("Habitaciones")) ret += 131072;
            if (optionsNames.Contains("Capacidad")) ret += 262144;
            if (optionsNames.Contains("Clientes")) ret += 524288;
            if (optionsNames.Contains("Vehiculos")) ret += 1048576;
            if (optionsNames.Contains("Operaciones")) ret += 2097152;
            if (optionsNames.Contains("Mapa")) ret += 4194304;
            if (optionsNames.Contains("Comision")) ret += 8388608;
            if (optionsNames.Contains("Usuarios")) ret += 16777216;
            if (optionsNames.Contains("Roles")) ret += 33554432;
            if (optionsNames.Contains("Grupos")) ret += 67108864;
            if (optionsNames.Contains("Idiomas")) ret += 134217728;
            if (optionsNames.Contains("Clasificaciones")) ret += 268435456;
            if (optionsNames.Contains("Proveedores")) ret += 536870912;
            return ret;
        }
    }
}