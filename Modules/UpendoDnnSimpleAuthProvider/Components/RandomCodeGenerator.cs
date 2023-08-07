using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace UpendoVentures.Auth.UpendoDnnSimpleAuthProvider.Components
{
    public class RandomCodeGenerator
    {
        public static string Generate()
        {
            string CharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder codeBuilder = new StringBuilder();

            for (int i = 0; i < 8; i++) // Generar un código de 8 caracteres
            {
                int randomIndex = random.Next(0, CharSet.Length);
                codeBuilder.Append(CharSet[randomIndex]);
            }

            return codeBuilder.ToString();

        }
    }
}