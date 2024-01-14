using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressApp;

public class MenuItem
{
    public MenuItem(char menuKey, HttpMethod verb,
        string relativeAddress,
        int concurrency,
        int totalDurationSec = 0)
    {
        this.MenuKey = menuKey;
        this.Verb = verb;
        this.RelativeAddress = relativeAddress;
        this.Concurrency = concurrency;
        TotalDurationSec = totalDurationSec;
    }

    public char MenuKey { get; set; }
    public HttpMethod Verb { get; set; }
    public string RelativeAddress { get; set; }
    public int Concurrency { get; set; }
    public int TotalDurationSec { get; }

    public override string ToString()
    {
        return $"{MenuKey}. {Verb} {RelativeAddress} ({Concurrency}) - {TotalDurationSec}";
    }

    public string ToStringTabular(params int[] columns)
    {
        if (columns.Length < 5) throw new ArgumentException(nameof(columns));
        var cc = Concurrency == 0 ? "random" : Concurrency.ToString();
        var dur = TotalDurationSec == 0 ? string.Empty : TotalDurationSec.ToString();

        return string.Join(" ",
            Pad(MenuKey, columns[0]),
            Pad(Verb, columns[1]),
            Pad(RelativeAddress, columns[2]),
            Pad(cc, columns[3]),
            Pad(dur, columns[4])
            );

        static string Pad<T>(T data, int pad) => data?.ToString()?.PadRight(pad) ?? string.Empty;
    }

}
