using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;
using XIVATM.Handlers;
using XIVATM.Helpers;
using XIVATM.Structs;

namespace XIVATM.UI.Settings;

public static class DefaultGilAmountsTab
{
    public static void DrawGilAmountsTab()
    {
        if (ImGui.BeginChild("Settings_UI##GilAmountsTab"))
        {
            DrawDefaultGilAmountToSend();

            ImGui.Spacing();

            DrawDefaultMaxGilsPerPlayerTimerange();

            ImGui.Spacing();

            DrawDefaultGlobalMaxGilsPerTimerange();

            ImGui.Spacing();

            DrawGlobalMaxGils();
        }

        ImGui.EndChild();
    }

    public static void DrawDefaultGilAmountToSend()
    {
        ChecksBeforeTrade canSendGils = CommandHandler.CheckCanSendGils();
        bool isValidGilAmountToSendFixed = canSendGils.IsValidGilAmountToSendFixed;
        bool isValidGilAmountToSendRandomRangeBoth = canSendGils.IsValidGilAmountToSendRandomRangeBoth;
        bool isValidGilAmountToSendRandomRangeLow = canSendGils.IsValidGilAmountToSendRandomRangeLow;

        if (ImGui.BeginChild("Settings_UI##GilAmountsTab##DefaultGilAmountToSend", new Vector2(-1f, 145f + (Service.Configuration!.DefaultMessageToSendAfterTransaction.ShouldSendMessage ? 30f : 0)), true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, "Default amount of gils to send :");

            if (Service.Configuration.DefaultGilAmountToSend.GilSenderMode == GilSenderMode.Fixed && !isValidGilAmountToSendFixed ||
                Service.Configuration.DefaultGilAmountToSend.GilSenderMode == GilSenderMode.RandomRange && (!isValidGilAmountToSendRandomRangeBoth || !isValidGilAmountToSendRandomRangeLow))
            {
                ImGui.SameLine();
                UIHelper.DrawRedCross();

                if (ImGui.IsItemHovered())
                {
                    UIHelper.StartColoringText(ImGuiColors.DalamudRed);

                    ImGui.SetTooltip((Service.Configuration.DefaultGilAmountToSend.GilSenderMode == GilSenderMode.RandomRange && !isValidGilAmountToSendRandomRangeLow) ?
                        ErrorMessages.InvalidGilAmountToSendRangeLowInvalid : ErrorMessages.InvalidGilAmountToSendFixedOrBothRangeInvalid);

                    UIHelper.EndColoringText();
                }
            }

            ImGui.Spacing();

            if (ImGui.RadioButton("Fixed Amount", Service.Configuration.DefaultGilAmountToSend.GilSenderMode == GilSenderMode.Fixed))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGilAmountToSend.GilSenderMode = GilSenderMode.Fixed; });

            ImGui.SameLine();

            if (ImGui.RadioButton("Random Range", Service.Configuration.DefaultGilAmountToSend.GilSenderMode == GilSenderMode.RandomRange))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGilAmountToSend.GilSenderMode = GilSenderMode.RandomRange; });

            switch (Service.Configuration.DefaultGilAmountToSend.GilSenderMode)
            {
                case GilSenderMode.Fixed:
                    {
                        int fixedAmount = Service.Configuration.DefaultGilAmountToSend.FixedAmount;

                        ImGui.SetNextItemWidth(200f);

                        if (ImGui.InputInt("##defaultGilAmountToSendFixedAmount", ref fixedAmount, 1000))
                        {
                            if (fixedAmount < 0)
                                fixedAmount = 0;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGilAmountToSend.FixedAmount = fixedAmount; });
                        }

                        UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"You will send a total of {fixedAmount:N0} gils each time a player uses you.");

                        break;
                    }
                case GilSenderMode.RandomRange:
                    {
                        int minAmount = Service.Configuration.DefaultGilAmountToSend.RangeLow;

                        ImGui.SetNextItemWidth(200f);

                        if (ImGui.InputInt("##defaultGilAmountToSendMinAmount", ref minAmount, 1000))
                        {
                            if (minAmount < 0)
                                minAmount = 0;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGilAmountToSend.RangeLow = minAmount; });
                        }

                        ImGui.SameLine();

                        int maxAmount = Service.Configuration.DefaultGilAmountToSend.RangeHigh;

                        ImGui.SetNextItemWidth(200f);

                        if (ImGui.InputInt("##defaultGilAmountToSendMaxAmount", ref maxAmount, 1000))
                        {
                            if (maxAmount < 0)
                                maxAmount = 0;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGilAmountToSend.RangeHigh = maxAmount; });
                        }

                        if (maxAmount < minAmount)
                        {
                            maxAmount = minAmount;

                            Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGilAmountToSend.RangeHigh = maxAmount; });
                        }

                        UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"You will send a random amount between {minAmount:N0} and {maxAmount:N0} gils each time a player uses you.");

                        break;
                    }
            }

            ImGui.Spacing();

            bool shouldSendMessageAfterTransaction = Service.Configuration.DefaultMessageToSendAfterTransaction.ShouldSendMessage;

            if (ImGui.Checkbox("Send a tell to the player after each transaction", ref shouldSendMessageAfterTransaction))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendAfterTransaction.ShouldSendMessage = shouldSendMessageAfterTransaction; });

            if (shouldSendMessageAfterTransaction)
            {
                string defaultMessageToSendAfterTransaction = Service.Configuration.DefaultMessageToSendAfterTransaction.Message;

                ImGui.AlignTextToFramePadding();

                ImGui.Text("Message to send :");

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1f);

                if (ImGui.InputTextWithHint("##defaultMessageToSendAfterTransaction", "Enter your message here...", ref defaultMessageToSendAfterTransaction, 400))
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendAfterTransaction.Message = defaultMessageToSendAfterTransaction; });
            }

        }
        
        ImGui.EndChild();
    }

    public static void DrawDefaultMaxGilsPerPlayerTimerange()
    {
        bool isValidDefaultGilAmountPlayerTimerange = CommandHandler.CheckCanSendGils().IsValidDefaultGilAmountPlayerTimerange;

        if (ImGui.BeginChild("Settings_UI##GilAmountsTab##DefaultMaxGilsPerPlayerTimerange", new Vector2(-1f, 162f + (Service.Configuration!.DefaultMessageToSendForMaxGilsPerPlayerTimerange.ShouldSendMessage ? 30f : 0)), true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, "Default amount of gils a player can withdraw from you within a time range :");

            ImGuiComponents.HelpMarker("This is the total amount of gils a single player can withdraw from you within a certain time range before ATM gets blocked for them.\n" +
                                       "Will start counting from the first time you deliver gils to that person, and until the timer resets.");

            if (!isValidDefaultGilAmountPlayerTimerange)
            {
                ImGui.SameLine();
                UIHelper.DrawRedCross();

                if (ImGui.IsItemHovered())
                {
                    UIHelper.StartColoringText(ImGuiColors.DalamudRed);
                    ImGui.SetTooltip(ErrorMessages.InvalidDefaultGilAmountPlayerTimerange);
                    UIHelper.EndColoringText();
                }
            }

            ImGui.Spacing();

            ImGui.AlignTextToFramePadding();

            ImGui.TextWrapped("Withdrawable amount :");

            ImGui.SameLine();

            int maxGilsPlayerTimerange = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils;

            ImGui.SetNextItemWidth(200f);

            if (ImGui.InputInt("##maxGilsPlayerTimerange", ref maxGilsPlayerTimerange, (maxGilsPlayerTimerange < 0 ? 1 : 1000)))
            {
                if (maxGilsPlayerTimerange < -1)
                    maxGilsPlayerTimerange = -1;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMaxGilsPerPlayerTimerange.MaxGils = maxGilsPlayerTimerange; });
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Set to -1 to allow unlimited gils to be withdrawn.");

            ImGui.AlignTextToFramePadding();

            ImGui.TextWrapped("Every :");

            ImGui.SameLine();

            int daysPlayerTimerange = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Days;
            int hoursPlayerTimerange = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Hours;
            int minutesPlayerTimerange = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Minutes;
            int secondsPlayerTimerange = Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Seconds;

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsPlayerTimerangeDays", ref daysPlayerTimerange, 0, 0))
            {
                if (daysPlayerTimerange < 0)
                    daysPlayerTimerange = 0;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Days = daysPlayerTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("days");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsPlayerTimerangeHours", ref hoursPlayerTimerange, 0, 0))
            {
                if (hoursPlayerTimerange < 0)
                    hoursPlayerTimerange = 0;

                if (hoursPlayerTimerange > 23)
                    hoursPlayerTimerange = 23;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Hours = hoursPlayerTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("hours");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsPlayerTimerangeMinutes", ref minutesPlayerTimerange, 0, 0))
            {
                if (minutesPlayerTimerange < 0)
                    minutesPlayerTimerange = 0;

                if (minutesPlayerTimerange > 59)
                    minutesPlayerTimerange = 59;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Minutes = minutesPlayerTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("minutes");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsPlayerTimerangeSeconds", ref secondsPlayerTimerange, 0, 0))
            {
                if (secondsPlayerTimerange < 0)
                    secondsPlayerTimerange = 0;

                if (secondsPlayerTimerange > 59)
                    secondsPlayerTimerange = 59;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMaxGilsPerPlayerTimerange.TimeRange.Seconds = secondsPlayerTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("seconds");

            UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"Any player will be able to withdraw {(maxGilsPlayerTimerange < 0 ? "an unlimited amount of gils" : $"a maximum of {maxGilsPlayerTimerange:N0} gils")} every {daysPlayerTimerange} days, {hoursPlayerTimerange} hours, {minutesPlayerTimerange} minutes and {secondsPlayerTimerange} seconds.");

            ImGui.Spacing();

            bool shouldSendMessageForMaxGilsPerPlayerTimerange = Service.Configuration.DefaultMessageToSendForMaxGilsPerPlayerTimerange.ShouldSendMessage;

            if (ImGui.Checkbox("Send a tell to the player if they reached the maximum gil cap", ref shouldSendMessageForMaxGilsPerPlayerTimerange))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendForMaxGilsPerPlayerTimerange.ShouldSendMessage = shouldSendMessageForMaxGilsPerPlayerTimerange; });

            if (shouldSendMessageForMaxGilsPerPlayerTimerange)
            {
                string defaultMessageToSendForMaxGilsPerPlayerTimerange = Service.Configuration.DefaultMessageToSendForMaxGilsPerPlayerTimerange.Message;

                ImGui.AlignTextToFramePadding();

                ImGui.Text("Message to send :");

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1f);

                if (ImGui.InputTextWithHint("##defaultMessageToSendForMaxGilsPerPlayerTimerange", "Enter your message here...", ref defaultMessageToSendForMaxGilsPerPlayerTimerange, 400))
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendForMaxGilsPerPlayerTimerange.Message = defaultMessageToSendForMaxGilsPerPlayerTimerange; });
            }

        }

        ImGui.EndChild();
    }

    public static void DrawDefaultGlobalMaxGilsPerTimerange()
    {
        ChecksBeforeTrade canSendGils = CommandHandler.CheckCanSendGils();

        bool isValidGlobalGilAmountTimerange = canSendGils.IsValidGlobalGilAmountTimerange;
        bool HasReachedTheGlobalLimitOfGilsPerTimerange = canSendGils.HasReachedTheGlobalLimitOfGilsPerTimerange;

        if (ImGui.BeginChild("Settings_UI##GilAmountsTab##DefaultGlobalMaxGilsPerTimerange", new Vector2(-1f, 162f + (Service.Configuration!.DefaultMessageToSendForGlobalMaxGilsPerTimerange.ShouldSendMessage ? 30f : 0)), true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, "Default global amount of gils withdrawable within a time range :");

            ImGuiComponents.HelpMarker("This is the total amount of gils that can be withdrawn globally within a certain time range before the ATM gets blocked.\n" +
                                       "Will start counting from the first time you deliver gils, and until the timer resets.");

            if (!isValidGlobalGilAmountTimerange || HasReachedTheGlobalLimitOfGilsPerTimerange)
            {
                ImGui.SameLine();
                UIHelper.DrawRedCross();

                if (ImGui.IsItemHovered())
                {
                    UIHelper.StartColoringText(ImGuiColors.DalamudRed);
                    ImGui.BeginTooltip();

                    if (!isValidGlobalGilAmountTimerange)
                        ImGui.Text(ErrorMessages.InvalidGlobalGilAmountTimerange);

                    if (HasReachedTheGlobalLimitOfGilsPerTimerange)
                    {
                        ImGui.Text(ErrorMessages.HasReachedGlobalGilCapPerTimerange);

                        // Calculate the time remaining and display it
                        if (Service.Configuration.GlobalWithdrawnGilsPerTimerange != null)
                        {
                            TimeSpan timeRemaining = Service.Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.End - DateTime.Now;
                            ImGui.Text($"Time remaining : {timeRemaining.Days:D2} days, {timeRemaining.Hours:D2} hours, {timeRemaining.Minutes:D2} minutes and {timeRemaining.Seconds:D2} seconds");
                        }
                    }

                    ImGui.EndTooltip();
                    UIHelper.EndColoringText();
                }
            }

            if (Service.Configuration.GlobalWithdrawnGilsPerTimerange != null)
            {
                ImGui.SameLine();

                bool ctrlPressed = ImGui.GetIO().KeyCtrl;

                if (ctrlPressed)
                {
                    if (ImGui.Button("Reset"))
                    {
                        CommonHelper.AddToHistory("The timer and counter corresponding to the global amount of gils withdrawable for a timerange has been reset.");

                        if (Service.Configuration.GlobalWithdrawnGilsPerTimerange != null)
                        {
                            string? timerId = Service.Configuration.GlobalWithdrawnGilsPerTimerange.TimeRangeDateTime.TimerId;

                            if (timerId != null)
                            {
                                Service.TimersList[timerId].Dispose();
                                Service.TimersList.Remove(timerId);
                            }
                        }

                        Service.Configuration.UpdateConfiguration(() => { Service.Configuration.GlobalWithdrawnGilsPerTimerange = null; });
                    }
                }
                else
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
                    ImGui.Button("Reset");
                    ImGui.PopStyleVar();
                }

                if (ImGui.IsItemHovered())
                {
                    TimeSpan timeRemaining = Service.Configuration.GlobalWithdrawnGilsPerTimerange!.TimeRangeDateTime.End - DateTime.Now;

                    ImGui.BeginTooltip();
                    ImGui.Text("Click this button to reset the counter and allow sending gils again in case the maximum cap is reached.\nHold the CTRL key to reset.");
                    ImGui.TextColored(ImGuiColors.DalamudOrange, $"Time remaining : {timeRemaining.Days:D2} days, {timeRemaining.Hours:D2} hours, {timeRemaining.Minutes:D2} minutes and {timeRemaining.Seconds:D2} seconds");
                    ImGui.EndTooltip();
                }
            }

            ImGui.Spacing();

            ImGui.AlignTextToFramePadding();

            ImGui.TextWrapped("Withdrawable amount :");

            ImGui.SameLine();

            int maxGilsGlobalTimerange = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils;

            ImGui.SetNextItemWidth(200f);

            if (ImGui.InputInt("##maxGilsGlobalTimerange", ref maxGilsGlobalTimerange, (maxGilsGlobalTimerange < 0 ? 1 : 1000)))
            {
                if (maxGilsGlobalTimerange < -1)
                    maxGilsGlobalTimerange = -1;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGlobalMaxGilsPerTimerange.MaxGils = maxGilsGlobalTimerange; });
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Set to -1 to allow unlimited gils to be withdrawn.");

            ImGui.AlignTextToFramePadding();

            ImGui.TextWrapped("Every :");

            ImGui.SameLine();

            int daysGlobalTimerange = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Days;
            int hoursGlobalTimerange = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Hours;
            int minutesGlobalTimerange = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Minutes;
            int secondsGlobalTimerange = Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Seconds;

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsGlobalTimerangeDays", ref daysGlobalTimerange, 0, 0))
            {
                if (daysGlobalTimerange < 0)
                    daysGlobalTimerange = 0;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Days = daysGlobalTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("days");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsGlobalTimerangeHours", ref hoursGlobalTimerange, 0, 0))
            {
                if (hoursGlobalTimerange < 0)
                    hoursGlobalTimerange = 0;

                if (hoursGlobalTimerange > 23)
                    hoursGlobalTimerange = 23;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Hours = hoursGlobalTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("hours");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsGlobalTimerangeMinutes", ref minutesGlobalTimerange, 0, 0))
            {
                if (minutesGlobalTimerange < 0)
                    minutesGlobalTimerange = 0;

                if (minutesGlobalTimerange > 59)
                    minutesGlobalTimerange = 59;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Minutes = minutesGlobalTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("minutes");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(50f);

            if (ImGui.InputInt("##maxGilsGlobalTimerangeSeconds", ref secondsGlobalTimerange, 0, 0))
            {
                if (secondsGlobalTimerange < 0)
                    secondsGlobalTimerange = 0;

                if (secondsGlobalTimerange > 59)
                    secondsGlobalTimerange = 59;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultGlobalMaxGilsPerTimerange.TimeRange.Seconds = secondsGlobalTimerange; });
            }

            ImGui.SameLine();

            ImGui.Text("seconds");

            UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"{(maxGilsGlobalTimerange < 0 ? "A global unlimited amount of gils" : $"A global maximum amount of {maxGilsGlobalTimerange:N0} gils")} can be withdrawn every {daysGlobalTimerange} days, {hoursGlobalTimerange} hours, {minutesGlobalTimerange} minutes and {secondsGlobalTimerange} seconds.");

            ImGui.Spacing();

            bool shouldSendMessageForMaxGilsPerGlobalTimerange = Service.Configuration.DefaultMessageToSendForGlobalMaxGilsPerTimerange.ShouldSendMessage;

            if (ImGui.Checkbox("Send a tell to the player if the gil cap has been reached for this timerange", ref shouldSendMessageForMaxGilsPerGlobalTimerange))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendForGlobalMaxGilsPerTimerange.ShouldSendMessage = shouldSendMessageForMaxGilsPerGlobalTimerange; });

            if (shouldSendMessageForMaxGilsPerGlobalTimerange)
            {
                string defaultMessageToSendForGlobalMaxGilsPerTimerange = Service.Configuration.DefaultMessageToSendForGlobalMaxGilsPerTimerange.Message;

                ImGui.AlignTextToFramePadding();

                ImGui.Text("Message to send :");

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1f);

                if (ImGui.InputTextWithHint("##defaultMessageToSendForGlobalMaxGilsPerTimerange", "Enter your message here...", ref defaultMessageToSendForGlobalMaxGilsPerTimerange, 400))
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendForGlobalMaxGilsPerTimerange.Message = defaultMessageToSendForGlobalMaxGilsPerTimerange; });
            }
        }
        
        ImGui.EndChild();
    }

    public static void DrawGlobalMaxGils()
    {
        ChecksBeforeTrade canSendGils = CommandHandler.CheckCanSendGils();

        bool isValidGlobalGilAmountWithdrawable = canSendGils.IsValidGlobalGilAmountWithdrawable;
        bool hasReachedTheGlobalLimitOfGils = canSendGils.HasReachedTheGlobalLimitOfGils;

        if (ImGui.BeginChild("Settings_UI##GilAmountsTab##GlobalMaxGils", new Vector2(-1f, 165f + (Service.Configuration!.DefaultMessageToSendForGlobalMaxGils.ShouldSendMessage ? 30f : 0f)), true, ImGuiWindowFlags.NoScrollbar))
        {
            UIHelper.TextWrappedColored(ImGuiColors.DalamudViolet, "Global amount of gils withdrawable (Bankrupcy Protection) :");
            ImGuiComponents.HelpMarker("This is the total amount of gils that can be withdrawn before the ATM gets blocked completely.\n" +
                                       "This exists to prevent bankrupcy. When the total amount of gils withdrawn reaches this amount, the ATM " +
                                       "will stop allowing gils to be sent until you click the reset button.");

            if (!isValidGlobalGilAmountWithdrawable || hasReachedTheGlobalLimitOfGils)
            {
                ImGui.SameLine();
                UIHelper.DrawRedCross();

                if (ImGui.IsItemHovered())
                {
                    UIHelper.StartColoringText(ImGuiColors.DalamudRed);
                    ImGui.BeginTooltip();

                    if (!isValidGlobalGilAmountWithdrawable)
                        ImGui.Text(ErrorMessages.InvalidGlobalGilAmountWithdrawable);

                    if (hasReachedTheGlobalLimitOfGils)
                        ImGui.Text(ErrorMessages.HasReachedGlobalGilCap);

                    ImGui.EndTooltip();
                    UIHelper.EndColoringText();
                }
            }

            ImGui.Spacing();

            int globalMaxGils = Service.Configuration.GlobalMaxGils;

            ImGui.AlignTextToFramePadding();

            ImGui.TextWrapped("Maximum total amount withdrawable :");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(200f);

            if (ImGui.InputInt("##globalMaxGils", ref globalMaxGils, (globalMaxGils < 0 ? 1 : 1000)))
            {
                if (globalMaxGils < -1)
                    globalMaxGils = -1;

                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.GlobalMaxGils = globalMaxGils; });
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Set to -1 to allow unlimited gils to be withdrawn.");

            ImGui.Spacing();

            string globalGilsSent = Service.Configuration.GlobalGilsSent.ToString();

            ImGui.AlignTextToFramePadding();

            ImGui.TextWrapped("Total amount withdrawn as of now :");

            ImGui.SameLine();

            ImGui.SetNextItemWidth(163f);

            ImGui.BeginDisabled();
            ImGui.InputText("##globalGilsSent", ref globalGilsSent, 200, ImGuiInputTextFlags.ReadOnly);
            ImGui.EndDisabled();

            ImGui.SameLine();

            bool ctrlPressed = ImGui.GetIO().KeyCtrl;

            if (ctrlPressed)
            {
                if (ImGui.Button("Reset##GlobalGilsSent"))
                {
                    CommonHelper.AddToHistory($"The global amount of gils withdrawn has been reset to 0 (previously {Service.Configuration.GlobalGilsSent:N0} gils).");
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.GlobalGilsSent = 0; });
                }
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
                ImGui.Button("Reset");
                ImGui.PopStyleVar();
            }
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Click this button to reset the counter and allow sending gils again in case the maximum cap is reached.\nHold the CTRL key to reset.");

            UIHelper.TextWrappedColored(ImGuiColors.DalamudOrange, $"{(globalMaxGils < 0 ? "An unlimited amount of gils" : $"A maximum amount of {globalMaxGils:N0} gils")} can be withdrawn before the ATM gets permanently blocked (Use the reset button to unblock).");

            ImGui.Spacing();

            bool shouldSendMessageForGlobalMaxGils = Service.Configuration.DefaultMessageToSendForGlobalMaxGils.ShouldSendMessage;

            if (ImGui.Checkbox("Send a tell to the player if the maximum global gil cap has been reached", ref shouldSendMessageForGlobalMaxGils))
                Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendForGlobalMaxGils.ShouldSendMessage = shouldSendMessageForGlobalMaxGils; });

            if (shouldSendMessageForGlobalMaxGils)
            {
                string defaultMessageToSendForGlobalMaxGils = Service.Configuration.DefaultMessageToSendForGlobalMaxGils.Message;

                ImGui.AlignTextToFramePadding();

                ImGui.Text("Message to send :");

                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1f);

                if (ImGui.InputTextWithHint("##defaultMessageToSendForGlobalMaxGils", "Enter your message here...", ref defaultMessageToSendForGlobalMaxGils, 400))
                    Service.Configuration.UpdateConfiguration(() => { Service.Configuration.DefaultMessageToSendForGlobalMaxGils.Message = defaultMessageToSendForGlobalMaxGils; });
            }
        }

        ImGui.EndChild();
    }
}
