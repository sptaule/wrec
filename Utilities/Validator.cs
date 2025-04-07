using System;

namespace wrec.Utilities
{
    public static class Validator
    {
        public static bool ValidateNumericField(string value, int min, int max, string fieldName, out int result)
        {
            if (!int.TryParse(value, out result) || result < min || result > max)
            {
                throw new ArgumentException($"{fieldName} doit être entre {min} et {max}");
            }
            return true;
        }

        public static bool ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Le chemin ne peut pas être vide");
            }
            return true;
        }
    }
}