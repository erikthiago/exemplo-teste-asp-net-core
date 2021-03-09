using System;
using System.Collections.Generic;
using System.Text;

namespace ExemploTestes.Test.Mocks
{
    public static class GeradorDadosCep
    {
        public static IEnumerable<object[]> OpcoesCep()
        {
            yield return new object[]
            {
              "70701000"
            };

            // Aqui coloque quantos forem necessários
            //yield return new object[]
            //{
            //       new SeuObjeto()
            //       {
            //            "70701000"
            //       }    
            //};
        }

    }
}
