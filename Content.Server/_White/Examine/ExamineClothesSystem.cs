using Content.Server.Access.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Robust.Shared.Enums;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Content.Shared.CCVar;

namespace Content.Server._White.Examine
{

    //^.^
    public sealed class ExamineClothesSystem : EntitySystem
    {
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly IdCardSystem _idCard = default!;
        [Dependency] private readonly IConsoleHost _consoleHost = default!;
        [Dependency] private readonly INetConfigurationManager _netConfigManager = default!;


        public override void Initialize()
        {
            SubscribeLocalEvent<ExaminableClothesComponent, ExaminedEvent>(HandleExamine);
        }

        private void SendNoticeMessage(ActorComponent actorComponent, string message)
        {
            var should = _netConfigManager.GetClientCVar(actorComponent.PlayerSession.Channel, CCVars.LogChatActions);

            if (should)
            {
                var formatedMessage = Loc.GetString("notice-command", ("fontsize", "10"), ("fontcolor", "aeabc4"), ("message", message));
                _consoleHost.RemoteExecuteCommand(actorComponent.PlayerSession, $"notice {formatedMessage}");
            }
        }

        private void HandleExamine(EntityUid uid, ExaminableClothesComponent comp, ExaminedEvent args)
        {
            var infoLines = new List<string>();

            var name = Name(uid);

            var ev = new SeeIdentityAttemptEvent();
            RaiseLocalEvent(uid, ev);
            if (ev.Cancelled)
            {
                if (_idCard.TryFindIdCard(uid, out var id) && !string.IsNullOrWhiteSpace(id.Comp.FullName))
                {
                    name = id.Comp.FullName;
                }
                else
                {
                    name = Loc.GetString("examine-uknown");
                }
            }
            infoLines.Add($"{Loc.GetString("examine-present")} [bold]{name}[/bold]!");

            var idInfoString = GetInfo(uid);
            if (!string.IsNullOrEmpty(idInfoString))
            {
                infoLines.Add(idInfoString);
                args.PushMarkup(idInfoString);
            }

            var slotLabels = new Dictionary<string, string>
            {
                { "head", "head-" },
                { "eyes", "eyes-" },
                { "mask", "mask-" },
                { "neck", "neck-" },
                { "ears", "ears-" },
                { "jumpsuit", "jumpsuit-" },
                { "pants", "pants-" },
                { "outerClothing", "outer-" },
                { "back", "back-" },
                { "gloves", "gloves-" },
                { "belt", "belt-" },
                { "shoes", "shoes-" }
            };

            foreach (var slotEntry in slotLabels)
            {
                var slotName = slotEntry.Key;
                var slotLabel = slotEntry.Value;

                if (_entityManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var appearanceComponent))
                {
                    switch (appearanceComponent.Gender)
                    {
                        case Gender.Male:
                            slotLabel += "he";
                            break;
                        case Gender.Neuter:
                            slotLabel += "it";
                            break;
                        case Gender.Epicene:
                            slotLabel += "they";
                            break;
                        case Gender.Female:
                            slotLabel += "she";
                            break;
                    }
                }

                if (!_inventorySystem.TryGetSlotEntity(uid, slotName, out var slotEntity))
                    continue;

                if (_entityManager.TryGetComponent<MetaDataComponent>(slotEntity, out var metaData))
                {
                    var item = $"{Loc.GetString(slotLabel)} [bold]{metaData.EntityName}[/bold].";
                    args.PushMarkup(item);
                    infoLines.Add(item);
                }
            }

            var combinedInfo = string.Join("\n", infoLines);

            if (TryComp(args.Examiner, out ActorComponent? actorComponent))
            {
                SendNoticeMessage(actorComponent, combinedInfo);
            }
        }

        private string GetInfo(EntityUid uid)
        {
            if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
            {
                // PDA
                if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) &&
                    TryComp<IdCardComponent>(pda.ContainedId, out var id))
                {
                    return GetNameAndJob(id);
                }
                // ID Card
                if (EntityManager.TryGetComponent(idUid, out id))
                {
                    return GetNameAndJob(id);
                }
            }
            return "";
        }

        private string GetNameAndJob(IdCardComponent id)
        {
            var jobSuffix = string.IsNullOrWhiteSpace(id.JobTitle) ? "" : $" ({id.JobTitle})";

            var val = string.IsNullOrWhiteSpace(id.FullName)
                ? Loc.GetString("access-id-card-component-owner-name-job-title-text",
                    ("jobSuffix", jobSuffix))
                : Loc.GetString("access-id-card-component-owner-full-name-job-title-text",
                    ("fullName", id.FullName),
                    ("jobSuffix", jobSuffix));

            return val;
        }
    }
}
