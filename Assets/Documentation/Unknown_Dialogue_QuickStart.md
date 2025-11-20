# Unknown Dialogue System - Quick Start Guide

## 5-Minute Setup

This guide will help you set up and test the Unknown Dialogue System in under 5 minutes.

## Step 1: Create Message Database (30 seconds)

1. Open Unity Editor
2. Click menu: **Protocol EMR > Dialogue > Create Message Database**
3. Confirm the database was created at: `Assets/Resources/Dialogue/UnknownMessageDatabase.asset`

The database is automatically populated with 90+ pre-written messages.

## Step 2: Add Systems to Scene (1 minute)

### Method A: Via GameManager (Recommended)

If you already have a GameManager in your scene:

1. Select the **GameManager** GameObject
2. In Inspector, find the **Unknown Dialogue Manager Prefab** field
3. Create a prefab:
   - Create new GameObject: "UnknownDialogueManager"
   - Add component: `UnknownDialogueManager`
   - Drag to prefab folder
   - Assign to GameManager field

### Method B: Manual Setup

Create three GameObjects in your scene:

**1. Unknown Dialogue Manager**
- Create Empty GameObject: "UnknownDialogueManager"
- Add Component: `UnknownDialogueManager`
- In Inspector, verify "Message Database" is auto-loaded

**2. Unknown Phone UI**
- Create UI Canvas (if not exists)
- Create Empty GameObject under Canvas: "UnknownPhoneUI"
- Add Component: `UnknownPhoneUI`
- Create UI elements:
  - Phone Panel (UI Panel)
  - Scroll View for messages
  - Message Container (Vertical Layout Group)
  - Unread Badge (UI Image + Text)

**3. Unknown HUD Overlay**
- Create Empty GameObject under Canvas: "UnknownHUDOverlay"
- Add Component: `UnknownHUDOverlay`
- Create UI elements:
  - Overlay Panel (UI Panel)
  - Message Text (TextMeshProUGUI)
  - Glitch Overlay (UI Image)

## Step 3: Test with Demo Controller (2 minutes)

1. Create Empty GameObject: "UnknownDialogueDemo"
2. Add Component: `UnknownDialogueDemoController`
3. Press Play
4. Press **F6** to open demo panel
5. Click buttons to test different message types

### Quick Tests

**Combat Test:**
- Press **1** or click "Test Combat Messages"
- Expected: Message appears on HUD overlay and/or phone

**Puzzle Test:**
- Press **2** or click "Test Puzzle Messages"
- Expected: Different message appears

**Exploration Test:**
- Press **3** or click "Test Exploration Messages"
- Expected: Exploratory message appears

## Step 4: Integrate with Your Game (1 minute)

Add trigger calls to your existing scripts:

```csharp
using ProtocolEMR.Core.Dialogue;

// In your combat script
public void OnPlayerHitEnemy(GameObject enemy, float damage)
{
    UnknownDialogueTriggers.TriggerPlayerHitNPC(enemy, damage);
}

// In your exploration script
public void OnSecretDiscovered(GameObject secret)
{
    UnknownDialogueTriggers.TriggerSecretFound(secret);
}

// In your puzzle script
public void OnPuzzleSolved()
{
    UnknownDialogueTriggers.TriggerPuzzleSolved(gameObject, perfect: true);
}
```

## Verify Installation

### Checklist

- [ ] Message database exists at `Assets/Resources/Dialogue/UnknownMessageDatabase.asset`
- [ ] UnknownDialogueManager in scene
- [ ] UnknownPhoneUI in scene (under Canvas)
- [ ] UnknownHUDOverlay in scene (under Canvas)
- [ ] Press Play - no errors in Console
- [ ] Press F6 - Demo panel appears
- [ ] Click test buttons - Messages appear
- [ ] Press C - Phone opens/closes

### Common Issues

**"Database not found" error:**
- Solution: Run `Protocol EMR > Dialogue > Create Message Database` menu

**Messages not appearing:**
- Check hint frequency slider (may be set to 0%)
- Verify UnknownDialogueManager exists in scene
- Check Console for errors

**Phone not opening:**
- Verify InputManager is in scene
- Check if 'C' key is bound in Input Actions

**HUD overlay not showing:**
- Verify Canvas exists and is active
- Check display mode in messages (Phone/HUD/Both)
- Ensure CanvasGroup component exists on overlay panel

## Keyboard Controls

**Demo Controls:**
- F6 - Toggle demo panel
- 1 - Test combat messages
- 2 - Test puzzle messages
- 3 - Test exploration messages
- 4 - Test mission messages
- 5 - Test narrative messages

**Game Controls:**
- C - Open/close phone (chat history)

## Settings

Access via SettingsManager:

```csharp
// Adjust hint frequency (0% to 100%)
SettingsManager.Instance.SetUnknownHintFrequency(0.75f);

// Change personality (0=Verbose, 1=Balanced, 2=Cryptic)
SettingsManager.Instance.SetUnknownPersonality(2);

// Enable/disable messages
SettingsManager.Instance.SetUnknownMessagesEnabled(true);
```

## Next Steps

1. **Customize Messages**: Edit `UnknownMessageDatabase.asset` in Inspector
2. **Add Integration**: Add trigger calls to your game systems
3. **Test Gameplay**: Play through your game and verify messages appear
4. **Adjust Settings**: Fine-tune hint frequency and personality
5. **Read Full Docs**: See `Unknown_Dialogue_System_README.md` for details

## Support

For full documentation, see:
- `Unknown_Dialogue_System_README.md` - Complete system guide
- `Sprint8_Summary.md` - Development summary
- Unity API comments in code

For troubleshooting, check the Console for error messages and verify all components are properly configured.

---

**Setup Time**: ~5 minutes
**Result**: Fully functional Unknown dialogue system ready for integration
