using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion.Model
{
    public enum SwitchCode
    {
        ERROR_VALIDATIONS = 450,  // VALIDATIONS
        ERROR_CUSTOMER_API = 451,  // CUSTOMER
        ERROR_SALES_API = 452,  // INVOICES, DOCUMENTS
        ERROR_EXCEPTIONS = 500,  // INVOICES, DOCUMENTS
    }
}
