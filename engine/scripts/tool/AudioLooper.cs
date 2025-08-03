using System;
using System.Globalization;
using Godot;
using System.IO;
using System.Linq;
using Array = Godot.Collections.Array;

namespace ChloePrime.MarioForever.Tool;

public enum AudioLooperMode
{
    TwoSlice,
    OneSliceWithOffset
}
public partial class AudioLooper : Control
{
    public AudioLooperMode Mode { get; set; } = AudioLooperMode.TwoSlice;
    
    private Array ConsoleOutput { get; } = [];

    [ExportGroup("References")] 
    [Export] private TextEdit QualityInput { get; set; }
    [Export] private OptionButton ModeMenu { get; set; }

    public override void _Ready()
    {
        base._Ready();
        GetWindow().FilesDropped += OnWindowFileDropped;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        CheckConsoleOutput();
    }

    private void CheckConsoleOutput()
    {
        foreach (var log in ConsoleOutput)
        {
            GD.Print(log.ToString());
        }

        if (ConsoleOutput.Count > 0)
        {
            ConsoleOutput.Clear();
        }
    }

    private void OnWindowFileDropped(string[] files)
    {
        foreach (var file in files)
        {
            LoopFile(file);
        }
    }

    private void LoopFile(string srcFilePath)
    {
        const string ffmpeg = @".\engine\tools\bin\ffmpeg.exe";
        
        // 读取配置
        Mode = (AudioLooperMode)ModeMenu.Selected;
        var quality = int.TryParse(QualityInput?.Text ?? "0", out var q) ? q : 0;
        
        
        var srcFile = new FileInfo(srcFilePath);
        if (srcFile.Directory is not { } folder)
        {
            GD.PushError("Dangling File");
            return;
        }

        // Compress to quality 0 OGG
        var tmpFilePath = srcFile.FullName[..^srcFile.Extension.Length] + "_temp.ogg";
        var cmdCompress = $"{ffmpeg} -y -i {WrapCmdArg(srcFilePath)} -vn -q:a {quality} -ar 44100 {WrapCmdArg(tmpFilePath)}";
        Exec(cmdCompress);

        // Loop Compressed OGG
        var cmdLooper = $"pymusiclooper export-points --path {WrapCmdArg(tmpFilePath)} --export-to TXT --fmt SECONDS";
        Exec(cmdLooper);

        if (!TryParseLoopPoints(folder, out var loopResult))
        {
            GD.PushError("Failed to parse loop result");
        }

        if (Mode == AudioLooperMode.OneSliceWithOffset)
        {
            FixPrecisionIssues(loopResult);   
        }
        
        string[] finalOutputs, cmdFfmpeg;
        switch (Mode)
        {
            case AudioLooperMode.TwoSlice:
                finalOutputs =
                [
                    srcFile.FullName[..^srcFile.Extension.Length] + "_intro.ogg",
                    srcFile.FullName[..^srcFile.Extension.Length] + "_loop.ogg"
                ];
                cmdFfmpeg =
                [
                    $"{ffmpeg} -y -t {loopResult[0]} -i {WrapCmdArg(tmpFilePath)} -c:a copy {WrapCmdArg(finalOutputs[0])}",
                    $"{ffmpeg} -y -ss {loopResult[0]} -i {WrapCmdArg(tmpFilePath)} -c:a copy -t {loopResult[1] - loopResult[0]} {WrapCmdArg(finalOutputs[1])}"
                ];
                break;
            case AudioLooperMode.OneSliceWithOffset:
                finalOutputs =
                [
                    srcFile.FullName[..^srcFile.Extension.Length] + ".ogg"
                ];
                cmdFfmpeg =
                [
                    $"{ffmpeg} -y -t {loopResult[1]} -i {WrapCmdArg(tmpFilePath)} -c:a copy {WrapCmdArg(finalOutputs[0])}"
                ];
                break;
            default:
                throw new IndexOutOfRangeException(nameof(Mode));
        }
        foreach (var cmd in cmdFfmpeg)
        {
            Exec(cmd);
        }

        if (Mode == AudioLooperMode.OneSliceWithOffset)
        {
            GD.Print($"Loop offset for {srcFile.Name} is {loopResult[0]:F3}");
            DisplayServer.ClipboardSet(loopResult[0].ToString(CultureInfo.InvariantCulture));
        }
        
        File.Delete(tmpFilePath);
    }

    private static void FixPrecisionIssues(double[] loopPoints)
    {
        var delta = Math.Round(loopPoints[0] * 1000) / 1000 - loopPoints[0];
        loopPoints[0] += delta;
        loopPoints[1] += delta;
    }

    private int Exec(string command)
    {
        GD.Print($"executing: {command}");
        var code = OS.Execute("cmd.exe", ["/c", command], ConsoleOutput, readStderr: true);
        CheckConsoleOutput();
        return code;
    }

    private static string WrapCmdArg(string path)
    {
        return '"' + path + '"';
    }

    private static bool TryParseLoopPoints(DirectoryInfo directory, out double[] result)
    {
        var outputFile = Path.Combine(directory.FullName,"LooperOutput/loops.txt");
        var outputFileDir = new FileInfo(outputFile).Directory?.FullName ?? "";
        result = new double[2];
        var split = File.ReadLines(outputFile).Last().Split(' ');
        var success = double.TryParse(split[0], out result[0]) && double.TryParse(split[1], out result[1]);
        
        if (success)
        {
            File.Delete(outputFile);

            if (Directory.GetFileSystemEntries(outputFileDir).IsEmpty())
            {
                Directory.Delete(outputFileDir);
            }
        }

        return success;
    }
}
