using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressApp;

public class Menu
{
    private string _address;
    private ServiceProvider _serviceProvider;
    private string _postPayload;
    private List<MenuItem> _menuItems = new List<MenuItem>();

    public Menu(ServiceProvider serviceProvider, string address)
    {
        _serviceProvider = serviceProvider;
        _address = address;

        // generate a random 1K string payload of readable characters
        Random rnd = new Random();
        _postPayload = new string(Enumerable.Range(0, 1024)
            .Select(n => (char)(rnd.Next(0, 96) + 32))
            .ToArray());

        _menuItems.Add(new MenuItem('1', HttpMethod.Get, "/weatherforecast", 1000));
        _menuItems.Add(new MenuItem('2', HttpMethod.Get, "/weatherforecast", 2500));
        _menuItems.Add(new MenuItem('3', HttpMethod.Get, "/weatherforecast", 4000));
        _menuItems.Add(new MenuItem('4', HttpMethod.Get, "/weatherforecast", 0, 60));
        _menuItems.Add(new MenuItem('5', HttpMethod.Get, "/weatherforecast", 0, 60 * 5));
    }

    public async Task Start()
    {
        Usage(default(ConsoleKeyInfo));
        ConsoleKeyInfo keyInfo = default(ConsoleKeyInfo);
        do
        {
            bool ignore = false;
            keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.C:
                    break;

                default:
                    ignore = true;
                    break;
            }

            var character = (char)((int)keyInfo.Key);
            var selectedMenu = _menuItems.FirstOrDefault(i => i.MenuKey == character);
            bool isStarted = false;
            if (selectedMenu != null)
            {
                if (selectedMenu.Verb == HttpMethod.Get)
                {
                    isStarted = true;
                    await MakeGet(selectedMenu);
                }
                else if (selectedMenu.Verb == HttpMethod.Post)
                {
                    isStarted = true;
                    await MakePost(selectedMenu);
                }

                if (isStarted)
                    Console.WriteLine($"\r\n{selectedMenu.Verb} {selectedMenu.RelativeAddress} has started");

                ignore = false;
            }

            if (!ignore) Usage(keyInfo);
        }
        while (keyInfo.Key != ConsoleKey.Q);
    }

    private void Usage(ConsoleKeyInfo key)
    {
        int[] columns = new[] { 2, 5, 26, 12, 15 };
        Console.Clear();
        Console.Write($"Pid = {Process.GetCurrentProcess().Id}      ");
        if (key.KeyChar != 0) Console.Write($"Last Command: {key.KeyChar}");
        Console.WriteLine();
        Console.WriteLine();
        TabWrite(columns, "#", "Verb", "Endpoint", "Concurrency", "Total Duration (sec)");
        foreach (var menuItem in _menuItems)
        {
            //Console.WriteLine(menuItem.ToString());
            Console.WriteLine(menuItem.ToStringTabular(columns));
        }
        Console.WriteLine($"C  Clear screen");
    }

    private void TabWrite(int[] columns, params string[] texts)
    {
        if (columns.Length < texts.Length) throw new ArgumentException(nameof(columns));
        for (int i = 0; i < texts.Length; i++)
        {
            Console.Write($"{Pad(texts[i], columns[i])} ");
        }
        Console.WriteLine();

        static string Pad(string data, int pad) => data.PadRight(pad);
    }

    private async Task MakeGet(MenuItem menuItem)
    {
        Func<StressClient, Task<bool>> requestMaker = async client =>
        {
            var result = await client.GetPage(menuItem.RelativeAddress);
            if (result)
                Console.Write(".");
            else
                Console.Write("X");
            return result;
        };

        await Execute(menuItem.Concurrency, menuItem.TotalDurationSec, requestMaker);
    }


    private async Task MakePost(MenuItem menuItem)
    {
        Func<StressClient, Task<bool>> requestMaker = async client =>
        {
            var result = await client.Post(menuItem.RelativeAddress, _postPayload);
            if (result)
                Console.Write(".");
            else
                Console.Write("X");
            return result;
        };

        await Execute(menuItem.Concurrency, menuItem.TotalDurationSec, requestMaker);
    }


    /// <summary>
    /// This method either call directly the func, or execute it in parallel
    /// using Task.Run
    /// Later it asks on the console when make the parallel call start together
    /// </summary>
    /// <param name="concurrency"></param>
    /// <param name="requestMaker"></param>
    /// <returns></returns>
    private async Task Execute(int concurrency, int totalDuractionSec, Func<StressClient, Task<bool>> requestMaker)
    {
        using var scope = _serviceProvider.CreateScope();

        if (concurrency == 1)
        {
            var client = scope.ServiceProvider.GetRequiredService<StressClient>();
            await requestMaker(client);
        }
        else if (concurrency == 0 && totalDuractionSec > 0) // random concurrency
        {
            Stopwatch sw = new();
            sw.Start();
            var evt = new ManualResetEventSlim();
            while (sw.Elapsed.TotalSeconds < totalDuractionSec)
            {
                var concur = Random.Shared.Next(100, 5000);
                Console.WriteLine();
                Console.WriteLine($"Random concurrency: {concur}");
                var requests = Enumerable.Range(0, concur)
                  .Select(_ =>
                  {
                      return Task.Run<bool>(() =>
                      {
                          var client = scope.ServiceProvider.GetRequiredService<StressClient>();
                          evt.Wait();
                          return requestMaker(client);
                      });
                  })
                  .ToArray();

                evt.Set();
                await Task.WhenAll(requests);
            }

            sw.Stop();
        }
        else
        {
            var evt = new ManualResetEventSlim();
            var requests = Enumerable.Range(0, concurrency)
              .Select(_ =>
              {
                  return Task.Run<bool>(() =>
                  {
                      var client = scope.ServiceProvider.GetRequiredService<StressClient>();
                      evt.Wait();
                      return requestMaker(client);
                  });
              })
              .ToArray();

            Console.WriteLine("Requests ready to go. Press any key to run them");
            Console.ReadKey();
            Console.WriteLine("GO!");
            evt.Set();

            await Task.WhenAll(requests);
        }
    }

}
