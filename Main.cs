using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
public partial class Main : Control
{
    LineEdit CommandLine;
    RichTextLabel OutputLabel;
    RichTextLabel WarningBox;
    string response;
    int whileCount = 0;
    String Output;
    bool enter = false;
    bool is_waiting = false;
    bool running = false;
    List<String> accepted = new List<String>() { "1", "2", "3", "4", "5", "6", " " };
    Dictionary<string, StoryNode> storyDict = new Dictionary<string, StoryNode>();
    List<string> filtered = new List<string>() { "" };
    StoryNode currentNode;
    StoryNode OneNode;
    bool editing = false;
    bool connected = false;
    string newOption;
    bool Titled = false;
    string newTitle;
    bool Descript = false;
    string newDescript;
    bool finished = false;
    string id;
    StoryNode editNode;
    List<String> emptyList = new List<string>() { };
    static Random rnd = new Random();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string json = File.ReadAllText("Data.json");
        storyDict = JsonConvert.DeserializeObject<Dictionary<string, StoryNode>>(File.ReadAllText("Data.json"));
        OutputLabel = GetTree().Root.GetNode<RichTextLabel>("Control/OutputLabel");
        CommandLine = GetTree().Root.GetNode<LineEdit>("Control/Console");
        WarningBox = GetTree().Root.GetNode<RichTextLabel>("Control/WarningBox");
        currentNode = storyDict["1"];
        printNode(currentNode);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {


    }

   public void input(int selection = 0)
    {
        response = CommandLine.Text;
        GD.Print(currentNode.Options.Count);
        int listlength = currentNode.Options.Count;
        if (editing)
        {
            newNode();
            return;
        }
        if (listlength != 0)
        {
            if (Int32.Parse(response) == listlength + 1)
            {
                newNode();
            }
            else if (Int32.Parse(response) <= currentNode.Options.Count)

            {
                currentNode = storyDict[currentNode.OptionIDs[Int32.Parse(response) - 1]];

                printNode(currentNode);

            }
            else
            {
                warn("Invalid Input");
            }
        }
        else if (Int32.Parse(response) == 1)
        {
            newNode();
        }

        CommandLine.Clear();
    }
    public async void warn(string message)
    {
        WarningBox.Text = message;
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        WarningBox.Text = "";
    }
    public void printNode(StoryNode node)
    {
        List<string> currentIds = new List<string>() { };
        string output = $"{node.Description} \n";
        if (currentNode.Options.Count() != 0 && currentNode.Options.Count() <= 6)
        {
            foreach (int i in Enumerable.Range(1, currentNode.Options.Count()))
            {
                output += $"{i}. {currentNode.Options[i - 1]} \n";
                currentIds.Add(i.ToString());
            }
            output += $"{currentNode.Options.Count + 1}. Create New Option...";
        }
        else if (currentNode.Options.Count() > 6)
        {
            List<string> possible = new List<string>() { };
            possible = currentNode.Options;
            foreach (int i in Enumerable.Range(1, 6))
            {
                int x = rnd.Next(possible.Count());
                output += $"{i}. {possible[x]} \n";
                currentIds.Add(possible[x]);
                possible.Remove(possible[x]);
            }
            output += "7. Create New Option...";
        }
        else
        {
            output += "1. Create New Option...";
        }
        print(output);
    }
    public void save()
    {
        File.WriteAllText("Data.json", "");
        string json = JsonConvert.SerializeObject(storyDict);
        File.WriteAllText("Data.json", json);
    }
    public void newNode()
    {
        response = CommandLine.Text;
        CommandLine.Clear();
        if (!editing)
        {
            editing = true;
            print("What is the connecting option?"); //fix this
            return;
        }
        else if (!connected)
        {
            connected = true;
            newOption = response;
            print("What is the title of this scene? (this is not shown to the player)");
            return;
        }
        else if (!Titled)
        {
            Titled = true;
            newTitle = response;
            print("Describe the scene");
            return;
        }
        else if (!Descript)
        {
            Descript = true;
            newDescript = response;
            print($"Is this correct? \n {newTitle} \n {newDescript} \n y or n");
            return;
        }
        else if (!finished)
        {
            if (response.ToLower() != "y" && response.ToLower() != "n")
            {
                warn("Invalid Input");
            }
            else if (response.ToLower() == "y")
            {
                currentNode.Options.Add(newOption);
                storyDict[currentNode.ID] = currentNode;
                string id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                id += newTitle;
                currentNode.OptionIDs.Add(id);
                currentNode = new StoryNode(newTitle, id, newDescript, new List<string>() { }, new List<string>() { });
                storyDict.Add(id, currentNode);
                save();
                editing = false;
                connected = false;
                Titled = false;
                Descript = false;
                finished = false;
                newOption = "";
                newTitle = "";
                newDescript = "";
                id = "";
                printNode(currentNode);
            }
            else
            {
                editing = false;
                connected = false;
                Titled = false;
                Descript = false;
                finished = false;
                newOption = "";
                newTitle = "";
                newDescript = "";
                id = "";
                printNode(currentNode);
            }
        }
    }
    public async void print(string message)
    {

        Output = "";
        OutputLabel.Clear();
        foreach (char i in message)
        {
            await ToSignal(GetTree().CreateTimer(0.01), "timeout");
            Output += i;
            OutputLabel.Text = Output;

        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Enter:
                    input();
                    break;
            }
        }
    }
}
public class StoryNode
{
    public string Title;
    public string Description;
    public List<string> Options;
    public List<string> OptionIDs;
    public string ID;

    public StoryNode(string title, string id, string description, List<string> options, List<string> optionIDs)
    {
        Title = title;
        ID = id;
        Description = description;
        Options = options;
        OptionIDs = optionIDs;


    }

}




