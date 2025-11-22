# Protocol EMR Code Editor - Quick Start

Get started with the editor in 5 minutes!

## Installation

```bash
npm install
npm run dev
```

The editor opens automatically at `http://localhost:5173`

## Your First Component

### 1. React Component

The editor comes with a default React component. Click the editor pane and modify:

```jsx
export default function App() {
  const [count, setCount] = React.useState(0)

  return (
    <div style={{ padding: '20px' }}>
      <h1>Counter: {count}</h1>
      <button onClick={() => setCount(count + 1)}>
        Increment
      </button>
    </div>
  )
}

const root = ReactDOM.createRoot(document.getElementById('root'))
root.render(<App />)
```

Watch the preview update in real-time! 🎉

### 2. Add a CSS File

1. Click **"+ New"** in Files panel
2. Select **"CSS"** from the dropdown
3. Edit styles:

```css
body {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  font-family: 'Segoe UI', sans-serif;
  min-height: 100vh;
}

#root {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100vh;
}

button {
  padding: 12px 24px;
  font-size: 16px;
  background: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  box-shadow: 0 4px 15px rgba(0,0,0,0.2);
  transition: transform 0.2s;
}

button:hover {
  transform: scale(1.05);
}
```

## Features Walkthrough

### Copy Code
- Hover over a file in the Files panel
- Click the **📋** icon
- Code copied to clipboard!

### Download Project
- Click **"⬇️ Download ZIP"** in toolbar
- Get all files in a ZIP with package.json
- Ready to use with npm/yarn

### Save to History
- Make changes to your code
- Click **"💾 Save to History"**
- Your version is saved!

### Restore from History
- Click any entry in the **Recent History** panel
- All files and settings are restored
- History persists after page reload

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+/` | Toggle comment |
| `Ctrl+Z` | Undo |
| `Ctrl+Shift+Z` | Redo |
| `Ctrl+F` | Find |
| `Alt+Up/Down` | Move line |
| `Shift+Alt+Up/Down` | Copy line |

See Monaco Editor docs for more.

## Supported Languages

| Language | File Extension | Use Case |
|----------|---|----------|
| JavaScript | `.js` | Vanilla JS |
| TypeScript | `.ts` | Type-safe JS |
| JSX | `.jsx` | React components |
| TSX | `.tsx` | React + TypeScript |
| Vue | `.vue` | Vue components |
| HTML | `.html` | Markup |
| CSS | `.css` | Styles |

## Quick Examples

### Vue Counter

Change language to **Vue** and paste:

```vue
<template>
  <div class="counter">
    <h1>Counter: {{ count }}</h1>
    <button @click="count++">Increment</button>
  </div>
</template>

<script>
import { ref } from 'vue'

export default {
  setup() {
    const count = ref(0)
    return { count }
  }
}
</script>

<style scoped>
.counter {
  text-align: center;
  padding: 20px;
}

button {
  padding: 10px 20px;
  font-size: 16px;
}
</style>
```

### HTML & CSS

Create new HTML file:

```html
<!DOCTYPE html>
<html>
<head>
  <title>My Page</title>
  <style>
    body { font-family: Arial; margin: 40px; }
    h1 { color: #333; }
  </style>
</head>
<body>
  <h1>Hello, World!</h1>
  <p>Edit the HTML to change this page.</p>
</body>
</html>
```

## Tips & Tricks

### Multiple Files
- Create separate files for different concerns
- Edit files independently
- Only the first file is previewed

### Live Reload
- Changes compile in <500ms
- No manual refresh needed
- Errors shown in preview

### Mobile Testing
- Get server URL from dev output
- Open on mobile device
- Test responsive design

### Error Messages
- Check **Preview Error** section
- Check browser DevTools console
- Syntax errors highlighted in editor

## Common Issues

**Preview is blank**
- Check preview error message
- Ensure first file has valid code
- Try refreshing the page

**Copy not working**
- Ensure browser allows clipboard access
- Try again in non-incognito mode
- Check browser console

**History not saving**
- Ensure localStorage is enabled
- Check browser storage quota
- Try clearing browser cache

## Next Steps

- Explore different language modes
- Build more complex components
- Download your projects as ZIP
- Share preview with colleagues

## Resources

- [Monaco Editor Docs](https://github.com/suren-atoyan/monaco-editor-react)
- [React Docs](https://react.dev)
- [Vue 3 Docs](https://vuejs.org)
- [Babel Docs](https://babeljs.io)

Happy coding! 🚀
