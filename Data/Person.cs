using System.Text.RegularExpressions;

namespace Sporty.Data;

public class Person
{
    public string Name { get; set; } = String.Empty;
    public string Gender { get; set; } = String.Empty;
    public int Age { get; set; } = default;
    public int Height { get; set; } = default;
    public int Weight { get; set; } = default;
    public string Email { get; set; } = String.Empty;
}

public static class PersonValidator
{
    private static Regex s_emailRegex = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");

    public static string Name(string text)
    {
        if (text.Length > 2 && text.Length < 64)
            return text;
        else
            throw new ArgumentException("Имя должно быть длиннее 2х символов и короче 20х");
    }
    public static string Gender(string text)
    {
        if (text is "Мужчина" || text is "Женщина")
            return text;
        else
            throw new ArgumentException("Вы должны быть мужчиной или женщиной");
    }
    public static int Age(string text)
    {
        if (int.TryParse(text, out var age) && age > 10 && age < 100)
            return age;
        else
            throw new ArgumentException("Вы должны быть старше 10 и младше 100 лет");
    }
    public static int Height(string text)
    {
        if (int.TryParse(text, out var height) && height > 100 && height < 250)
            return height;
        else
            throw new ArgumentException("Вы должны быть выше 100 и ниже 250 сантиметров");
    }
    public static int Weight(string text)
    {
        if (int.TryParse(text, out var weight) && weight > 30 && weight < 200)
            return weight;
        else
            throw new ArgumentException("Ваш должен быть больше 30 и меньше 200 килограмм");
    }
    public static string Email(string text)
    {
        Match match = s_emailRegex.Match(text);
        if (match.Success)
            return text;
        else
            throw new ArgumentException("Введёна некорректная электронная почта");
    }
}