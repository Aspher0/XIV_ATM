using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;
using XIVATM.Helpers;
using XIVATM.Models;
using XIVATM.Structs;

namespace XIVATM.UI.Settings;

public static class DefaultTriggerPhrasesTab
{
    public static void DrawDefaultTriggerPhrasesTab()
    {
        UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "If you receive a tell and *ANY* of these triggers are found in the message, the ATM will trigger.\nIt is not a \"AND\" condition, each trigger is independent from the others.");

        if (ImGui.BeginChild("DefaultTriggerPhrasesTab##TriggerPhrases", new Vector2(-1f, -ImGui.GetFrameHeightWithSpacing()), true))
        {
            // Foreach trigger phrases in Service.Configuration.DefaultTriggerPhrases, draw, in a line that must NOT overflow (to prevent horizontal scrolling), an inputwithint for the phrase, a combo box with 3 choices (Triggerplacementinmessage) and a button to remove the trigger.
            foreach (var triggerPhrase in Service.Configuration!.DefaultTriggerPhrases)
            {
                var index = Service.Configuration.DefaultTriggerPhrases.IndexOf(triggerPhrase);

                if (ImGui.BeginChild($"DefaultTriggerPhrases##{triggerPhrase.UniqueId}", new Vector2(-1f, 40f), true))
                {
                    if (triggerPhrase.Phrase == string.Empty)
                    {
                        ImGui.AlignTextToFramePadding();
                        UIHelper.DrawRedCross();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text("The trigger phrase cannot be empty.");

                            UIHelper.StartColoringText(ImGuiColors.DalamudRed);
                            ImGui.Text("This trigger will be skipped.");
                            UIHelper.EndColoringText();

                            ImGui.EndTooltip();
                        }

                        ImGui.SameLine();
                    }

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"Must");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100f);

                    if (ImGui.BeginCombo($"##TriggerPlacementInMessage{triggerPhrase.UniqueId}", triggerPhrase.PlacementInMessage.ToString()))
                    {
                        foreach (var item in Enum.GetValues(typeof(TriggerPlacementInMessage)))
                        {
                            bool isSelected = (TriggerPlacementInMessage)item == triggerPhrase.PlacementInMessage;

                            if (ImGui.Selectable(item.ToString(), isSelected))
                            {
                                Service.Configuration.UpdateConfiguration(() =>
                                {
                                    triggerPhrase.PlacementInMessage = (TriggerPlacementInMessage)item;
                                });
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();

                    float remainingWidth = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(remainingWidth - 130f - 110f);

                    string phrase = triggerPhrase.Phrase;

                    if (ImGui.InputTextWithHint($"##TriggerPhrase{triggerPhrase.UniqueId}", "Trigger phrase ...", ref phrase, 250))
                    {
                        Service.Configuration.UpdateConfiguration(() =>
                        {
                            triggerPhrase.Phrase = phrase;
                        });
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("The phrase that will trigger the ATM.");
                        ImGui.EndTooltip();
                    }

                    ImGui.SameLine();

                    ImGui.SetNextItemWidth(130f);

                    if (ImGui.BeginCombo($"##CaseSensitivity{triggerPhrase.UniqueId}", triggerPhrase.CaseSensitivity.ToString()))
                    {
                        foreach (var item in Enum.GetValues(typeof(TriggerCaseSensitivity)))
                        {
                            bool isSelected = (TriggerCaseSensitivity)item == triggerPhrase.CaseSensitivity;

                            if (ImGui.Selectable(item.ToString(), isSelected))
                            {
                                Service.Configuration.UpdateConfiguration(() =>
                                {
                                    triggerPhrase.CaseSensitivity = (TriggerCaseSensitivity)item;
                                });
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.SameLine();

                    bool ctrlPressed = ImGui.GetIO().KeyCtrl;

                    if (ctrlPressed)
                    {
                        if (ImGui.Button($"Delete Trigger##{triggerPhrase.UniqueId}"))
                        {
                            Service.Configuration.UpdateConfiguration(() =>
                            {
                                Service.Configuration.DefaultTriggerPhrases.Remove(triggerPhrase);
                            });
                        }
                    }
                    else
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
                        ImGui.Button("Delete Trigger");
                        ImGui.PopStyleVar();
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Hold CTRL to delete.");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndChild();
            }
        }

        ImGui.EndChild();

        DrawDefaultTriggerPhrasesActionBar();
    }

    public static void DrawDefaultTriggerPhrasesActionBar()
    {
        if (ImGui.Button("Add a new trigger"))
        {
            Service.Configuration!.UpdateConfiguration(() =>
            {
                TriggerPhrase newTriggerPhrase = new TriggerPhrase("");
                Service.Configuration.DefaultTriggerPhrases.Add(newTriggerPhrase);
            });
        }
    }
}
