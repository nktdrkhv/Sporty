namespace Sporty.Models;

public class Person
{
    public string Name { get; set; }
    public bool Gender { get; set; }
    public int Age { get; set; }
    public int Weight { get; set; }
    public string Email { get; set; }

    public Person(string name, bool gender, int age, int weight, string email)
    {
        Name = name;
        Gender = gender;
        Age = age;
        Weight = weight;
        Email = email;
    }
}