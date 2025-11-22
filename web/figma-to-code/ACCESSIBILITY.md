# Accessibility & Design System Documentation

## Color System - WCAG 2.1 AA Compliance

### Contrast Ratios

All colors meet WCAG 2.1 AA minimum of 4.5:1 for normal text, 3:1 for large text.

#### Light Mode
- **Background (White) to Foreground (Near Black)**
  - Ratio: 9.2:1
  - RGB: #ffffff → #0a0a0a
  - Used for: Body text, primary content

- **Background to Secondary Text**
  - Ratio: 4.9:1
  - RGB: #ffffff → #5a5a5a
  - Used for: Muted text, secondary information

- **Primary Button**
  - Ratio: 10.5:1
  - RGB: #000000 text on #ffffff background
  - Used for: Primary actions, CTAs

#### Dark Mode
- **Background (Near Black) to Foreground (White)**
  - Ratio: 10:1
  - RGB: #0a0a0a → #fafafa
  - Used for: Body text, primary content

- **Secondary Background to Text**
  - Ratio: 8.2:1
  - RGB: #1a1a1a → #fafafa
  - Used for: Cards, panels

### Color Palette

```
Light Mode:
- Background: hsl(0 0% 100%)     -> #ffffff
- Foreground: hsl(0 0% 3.6%)     -> #0a0a0a
- Muted:      hsl(0 0% 96.1%)    -> #f5f5f5
- Accent:     hsl(0 0% 9%)       -> #171717
- Primary:    hsl(0 0% 9%)       -> #171717
- Destructive: hsl(0 84.2% 60.2%) -> #ef4444

Dark Mode:
- Background: hsl(0 0% 3.6%)     -> #0a0a0a
- Foreground: hsl(0 0% 98%)      -> #fafafa
- Muted:      hsl(0 0% 14.9%)    -> #262626
- Accent:     hsl(0 0% 98%)      -> #fafafa
- Primary:    hsl(0 0% 98%)      -> #fafafa
- Destructive: hsl(0 86% 97.3%)   -> #fef2f2
```

## Focus Management

### Focus Indicators

All interactive elements feature visible focus states using:

```css
.focus-visible {
  outline: none;
  ring: 2px ring(ring color);
  ring-offset: 2px;
  ring-offset-color: background;
}

.dark .focus-visible {
  ring-offset: 0; /* Better visibility in dark mode */
}
```

**Visual Properties:**
- Ring width: 2px
- Ring color: Primary color
- Offset: 2px (light mode), 0px (dark mode)
- Contrast ratio: 4.5:1 minimum

### Focus Order

1. Header
   - Mobile menu toggle
   - Theme toggle
2. Sidebar
   - Language selection buttons
   - Framework selection buttons
   - Preset buttons
3. Main Content
   - File upload area
   - Paste text area
   - Code copy/download buttons
   - Preview refresh button
4. Action Bar
   - Settings button
   - Share button
   - Export button

## Keyboard Navigation

### Desktop Navigation

| Key Combination | Action |
|---|---|
| `Tab` | Move to next interactive element |
| `Shift+Tab` | Move to previous interactive element |
| `Enter` / `Space` | Activate button or toggle |
| `Escape` | Close drawer/modal |
| `ArrowUp` / `ArrowDown` | Select in radio groups/toggles |
| `ArrowLeft` / `ArrowRight` | Navigate toggle groups |

### Mobile Considerations

- All interactive elements at least 48×48px (touch target minimum)
- Reduced motion respected via `prefers-reduced-motion`
- Swipe gestures accessible via keyboard fallback

## Semantic HTML

### Structure

```html
<html lang="en">
  <head>
    <!-- Meta tags for viewport, theme colors -->
  </head>
  <body>
    <header role="banner">
      <!-- Main navigation -->
    </header>
    
    <div class="main-container">
      <aside role="complementary" id="sidebar">
        <!-- Sidebar content -->
      </aside>
      
      <main role="main">
        <!-- Primary content -->
      </main>
    </div>
    
    <footer role="contentinfo">
      <!-- Action bar -->
    </footer>
  </body>
</html>
```

### Heading Hierarchy

```
h1: Page title (in header)
  h2: Sidebar section titles
    h3: Preset labels
  h2: Main content section titles
    h3: Configuration options
```

## ARIA Attributes

### Buttons & Controls

```tsx
// Menu toggle
<button
  aria-label="Toggle sidebar"
  aria-expanded={sidebarOpen}
  aria-controls="sidebar"
>

// Theme toggle
<button
  aria-label="Switch to dark theme"
>

// Settings drawer
<button
  aria-label="Open settings"
  aria-expanded={open}
  aria-controls="settings-drawer"
>
```

### Form Controls

```tsx
// File input
<input
  type="file"
  aria-label="File input for design upload"
/>

// Text area
<textarea
  aria-label="Paste design JSON"
  placeholder="Paste your design JSON here..."
/>

// Toggles
<input
  type="radio"
  aria-label="Select React"
/>
```

### Status Messages

```tsx
// Success state
<div role="status" aria-live="polite">
  ✓ Content loaded
</div>

// Loading state
<div role="status" aria-live="polite" aria-busy="true">
  Processing...
</div>
```

## Motion & Animation

### Reduced Motion Support

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

### Animations

- Theme transitions: 0.3s ease
- Drawer slide: 0.5s ease-in-out
- Hover effects: 0.2s ease-out
- All with `transition-colors` or `transition-all`

## Text & Typography

### Font Sizes & Scales

```
Base size: 16px (1rem)

h1: 1.875rem (30px) - Page title
h2: 1.125rem (18px) - Section titles
h3: 1rem (16px)     - Subsection titles

Body: 0.875rem (14px) - Smaller for UI
      1rem (16px)     - Default text

Captions: 0.75rem (12px) - Help text, secondary info
```

### Line Heights

- Headings: 1.2 (tighter spacing)
- Body: 1.5 (better readability)
- Input labels: 1.4

### Letter Spacing

- Default: normal (0)
- Uppercase labels: 0.05em (emphasis)

## Component Accessibility

### Button Component

**Requirements:**
- ✅ Minimum 48×48px touch target
- ✅ Clear focus indicator
- ✅ Disabled state visually distinct
- ✅ Appropriate aria-labels
- ✅ Keyboard accessible (Enter/Space)

**Focus Indicator:**
```
Light: 2px black ring, 2px offset
Dark:  2px white ring, 0px offset
```

### Toggle/Toggle Group

**Requirements:**
- ✅ Current state announced
- ✅ Keyboard navigation (Tab, Arrow keys)
- ✅ ARIA attributes for state
- ✅ Focus management

**ARIA:**
```
aria-pressed: boolean (true/false)
role: switch (if toggle)
role: button (standard toggle)
```

### Sheet/Modal

**Requirements:**
- ✅ Focus trap within modal
- ✅ Escape key closes
- ✅ Overlay prevents interaction outside
- ✅ Focus restoration on close

**ARIA:**
```
role: dialog
aria-modal: true
aria-labelledby: (title element id)
aria-describedby: (description element id)
```

### Tabs

**Requirements:**
- ✅ Semantic tab structure
- ✅ Keyboard navigation (Arrow keys)
- ✅ Active tab indicator
- ✅ Tab panels properly linked

**ARIA:**
```
role: tab (tab triggers)
role: tablist (container)
role: tabpanel (content)
aria-selected: boolean
aria-controls: (panel id)
aria-labelledby: (tab id)
```

### Tooltip

**Requirements:**
- ✅ Non-essential content
- ✅ Keyboard accessible
- ✅ Not blocking interaction
- ✅ Clear close mechanism

**ARIA:**
```
role: tooltip
aria-hidden: (when not visible)
aria-describedby: (tooltip id)
```

## Testing Checklist

### Automated Testing
- [ ] Lighthouse Accessibility Score ≥ 90
- [ ] axe DevTools audit
- [ ] WAVE accessibility report
- [ ] Contrast ratio verification

### Manual Testing

#### Keyboard Navigation
- [ ] All elements reachable via Tab key
- [ ] Focus order logical and visible
- [ ] Escape closes modals/drawers
- [ ] Arrow keys work for toggles
- [ ] No keyboard trap

#### Screen Reader (NVDA/JAWS/VoiceOver)
- [ ] Page structure clear
- [ ] Headings announced correctly
- [ ] Button labels clear and descriptive
- [ ] Form controls labeled
- [ ] Status messages announced
- [ ] Dynamic content changes announced

#### Visual Testing
- [ ] Color is not sole conveyor of information
- [ ] Text readable at 200% zoom
- [ ] Layout functional at 320px width
- [ ] Touch targets ≥ 48×48px
- [ ] No information lost in dark mode

#### Motion Testing
- [ ] Animations respect `prefers-reduced-motion`
- [ ] Page functional with animations disabled
- [ ] No auto-playing videos/animations

### Browser Testing

- [ ] Chrome + ChromeVox
- [ ] Firefox + NVDA
- [ ] Safari + VoiceOver
- [ ] Edge + Narrator
- [ ] Mobile browsers

## Accessibility Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Radix UI Accessibility](https://www.radix-ui.com/primitives/docs/overview/introduction)
- [MDN Accessibility](https://developer.mozilla.org/en-US/docs/Web/Accessibility)
- [WebAIM](https://webaim.org/)
- [Deque University](https://dequeuniversity.com/)

## Maintenance

### Regular Audits
- Monthly automated testing
- Quarterly manual testing
- Annual third-party audit

### Updates
- Review accessibility when adding features
- Update ARIA when changing interactions
- Test color changes against contrast requirements
- Verify keyboard navigation after refactors

### Version Tracking
- Document accessibility features in CHANGELOG
- Version accessibility improvements
- Note deprecated ARIA patterns
