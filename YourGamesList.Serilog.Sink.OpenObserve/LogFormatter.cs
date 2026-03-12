using System;
using System.Globalization;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;

namespace YourGamesList.Serilog.Sink.OpenObserve;

public class LogFormatter : ITextFormatter
{
    private readonly JsonValueFormatter _valueFormatter;
    private const string TypeTagName = "$type";

    public LogFormatter(JsonValueFormatter? valueFormatter = null)
    {
        _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: TypeTagName);
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);

        output.Write("{\"@t\":\"");
        output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));
        output.Write("\",\"@msg\":");

        var message = logEvent.RenderMessage(CultureInfo.InvariantCulture);
        JsonValueFormatter.WriteQuotedJsonString(message, output);

        output.Write(",\"@mt\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

        var id = EventIdHash.Compute(logEvent.MessageTemplate.Text);
        output.Write(",\"@i\":\"");
        output.Write(id.ToString("x8"));
        output.Write("\",\"@level\":\"");
        output.Write(logEvent.Level.ToString());
        output.Write('\"');

        if (logEvent.Exception != null)
        {
            output.Write(",\"@x\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
        }

        foreach (var property in logEvent.Properties)
        {
            output.Write(',');
            var name = property.Key;
            if (name.Length > 0 && name[0] == '@')
            {
                name = '@' + name;
            }

            JsonValueFormatter.WriteQuotedJsonString(name, output);
            output.Write(':');
            _valueFormatter.Format(property.Value, output);
        }

        output.Write('}');
        output.WriteLine();
    }
}