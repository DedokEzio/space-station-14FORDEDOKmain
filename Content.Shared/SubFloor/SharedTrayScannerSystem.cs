using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Shared.SubFloor;

public abstract class SharedTrayScannerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public const float SubfloorRevealAlpha = 0.8f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TrayScannerComponent, ComponentGetState>(OnTrayScannerGetState);
        SubscribeLocalEvent<TrayScannerComponent, ComponentHandleState>(OnTrayScannerHandleState);
        SubscribeLocalEvent<TrayScannerComponent, ActivateInWorldEvent>(OnTrayScannerActivate);
        SubscribeLocalEvent<WearableTrayScannerComponent, GotEquippedEvent>(OnTrayScannerEquip);
        SubscribeLocalEvent<WearableTrayScannerComponent, GotUnequippedEvent>(OnTrayScannerUnequip);
    }

    private void OnTrayScannerEquip(EntityUid uid, WearableTrayScannerComponent scanner, GotEquippedEvent args)
    {
        SetWearableScannerEnabled(uid, !scanner.Enabled, scanner);
    }

    private void OnTrayScannerUnequip(EntityUid uid, WearableTrayScannerComponent scanner, GotUnequippedEvent args)
    {
        SetWearableScannerEnabled(uid, !scanner.Enabled, scanner);
    }

    private void OnTrayScannerActivate(EntityUid uid, TrayScannerComponent scanner, ActivateInWorldEvent args)
    {
        SetScannerEnabled(uid, !scanner.Enabled, scanner);
    }

    private void SetScannerEnabled(EntityUid uid, bool enabled, TrayScannerComponent? scanner = null)
    {
        if (!Resolve(uid, ref scanner) || scanner.Enabled == enabled)
            return;

        scanner.Enabled = enabled;
        Dirty(uid, scanner);

        // We don't remove from _activeScanners on disabled, because the update function will handle that, as well as
        // managing the revealed subfloor entities

        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            _appearance.SetData(uid, TrayScannerVisual.Visual, scanner.Enabled ? TrayScannerVisual.On : TrayScannerVisual.Off, appearance);
        }
    }
    private void SetWearableScannerEnabled(EntityUid uid, bool enabled, WearableTrayScannerComponent? scanner = null)
    {
        if (!Resolve(uid, ref scanner) || scanner.Enabled == enabled)
            return;

        scanner.Enabled = enabled;
        Dirty(scanner);

        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            _appearance.SetData(uid, TrayScannerVisual.Visual, scanner.Enabled ? TrayScannerVisual.On : TrayScannerVisual.Off, appearance);
        }
    }


    private void OnTrayScannerGetState(EntityUid uid, TrayScannerComponent scanner, ref ComponentGetState args)
    {
        args.State = new TrayScannerState(scanner.Enabled, scanner.Range);
    }

    private void OnTrayScannerHandleState(EntityUid uid, TrayScannerComponent scanner, ref ComponentHandleState args)
    {
        if (args.Current is not TrayScannerState state)
            return;

        scanner.Range = state.Range;
        SetScannerEnabled(uid, state.Enabled, scanner);
    }
}

[Serializable, NetSerializable]
public enum TrayScannerVisual : sbyte
{
    Visual,
    On,
    Off
}
