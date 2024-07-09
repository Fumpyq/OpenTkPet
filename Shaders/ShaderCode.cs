using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public abstract class ShaderCode
    {
        public const string GlslVersion = "#version 460 core";
        public string GetTop() => @$"{GlslVersion}
{GetTops()}";
            public string GetMiddle() => $"main(){{{GetMain()}}}";
        public abstract string GetMain();
        public abstract string GetTops();
        
        public string GetCode()=> GetTops() + GetMain();
    }
}
