// See https://aka.ms/new-console-template for more information
using ConsoleApp1_Pet;

Console.WriteLine("Hello, World!");





using (MainGameWindow game = new MainGameWindow(1920,1080, "/./"))
{
    Console.WriteLine($"Ver IS:{game.APIVersion}");
    game.VSync = OpenTK.Windowing.Common.VSyncMode.On;
    game.Run();
}
