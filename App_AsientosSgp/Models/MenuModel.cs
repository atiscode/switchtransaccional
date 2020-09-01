using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class MenuModel:IComparable<MenuModel>
    {
        public enum TipoElemento
        {
            Carpeta,
            Elemento
        }
        public TipoElemento Tipo { get; set; }
        public MenuModel MenuPadre { get; set; }
        public string Nombre { get; private set; }
        public string Display { get; private set; }
        public string Key { get; private set; }
        public bool Selected { get; set; }
        public List<string> Routers { get; set; }
        public MenuModel(MenuModel padre, string nombre, string display, string Key, TipoElemento Tipo,IEnumerable<string> routers=null)
        {
            this.MenuPadre = padre;
            this.Nombre = nombre;
            this.Display = display;
            this.Key = Key;
            this.Tipo = Tipo;
            if(routers!=null)
            {
                var temp = routers.ToList();
                temp.Add(Nombre.NormalizeForSearch());
                this.Routers = temp;
            }
            else
            {
                this.Routers = new List<string> {Nombre.NormalizeForSearch()};
            }
           
        }
        public int CompareTo(MenuModel other)
        {
            return String.Compare(Nombre.ToLower(), other.Nombre.ToLower(), StringComparison.Ordinal);
        }
    }
    public class RouteModel
    {
        public string MainFolder { get; set; }
        public string ActionKey { get; set; }
    }
   
}