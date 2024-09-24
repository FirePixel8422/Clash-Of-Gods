using System.Collections.Generic;

public class RomanNumeralConverter
{
    public static string IntToRoman(int num)
    {
        // Define a list of Roman numerals and their corresponding integer values
        var romanNumerals = new (int value, string symbol)[]
        {
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
            (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
            (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
        };

        // StringBuilder to accumulate the resulting Roman numeral
        var result = new System.Text.StringBuilder();

        // Loop through each pair of value and symbol in the Roman numeral system
        foreach (var (value, symbol) in romanNumerals)
        {
            // Append the Roman numeral symbol to the result while the number is greater than the value
            while (num >= value)
            {
                result.Append(symbol);
                num -= value; // Reduce the number
            }
        }

        return result.ToString();
    }
}
