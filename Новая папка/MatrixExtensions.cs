using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class MatrixExtensions
    {
        public static string ToTransformString(this Matrix4 model)
        {
            var Pos     =  model.ExtractTranslation();
            var Rot     =  model.ExtractRotation();
            var Scale   =  model.ExtractScale();
            return $"P:{Pos},Q:{Rot.ToEulerAngles()},S:{Scale}";

        }
     
    }
}
