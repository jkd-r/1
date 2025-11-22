# Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Prerequisites
- **Node.js** 18+ (check with `node --version`)
- **npm** (comes with Node.js)

### Installation

1. **Navigate to the project**:
   ```bash
   cd web/figma-to-code
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Start the development server**:
   ```bash
   npm run dev
   ```

4. **Open your browser**:
   Navigate to `http://localhost:5173`

### Basic Usage

1. **Upload Figma Design**:
   - Click the upload area or drag & drop your Figma JSON file
   - File should be exported from Figma as "JSON" format

2. **Choose Your Framework**:
   - Select from: React, Vue, Angular, HTML, Tailwind CSS, or Bootstrap
   - Each stack generates framework-specific code

3. **Generate Code**:
   - Click "Generate Code"
   - View the generated code in the Output tab
   - Check the Preview tab (for HTML output)
   - Access previous conversions in the History tab

### Testing the Application

```bash
# Run all tests
npm run test

# Run performance tests
npm run test:performance

# Check Lighthouse scores
npm run lighthouse:dev
```

### Example Figma JSON Structure

```json
{
  "document": {
    "children": [
      {
        "id": "1:1",
        "name": "My Button",
        "type": "RECTANGLE",
        "visible": true,
        "absoluteBoundingBox": {
          "x": 0, "y": 0, "width": 120, "height": 40
        },
        "fills": [
          {
            "type": "SOLID",
            "visible": true,
            "color": { "r": 0.2, "g": 0.6, "b": 1 }
          }
        ],
        "cornerRadius": 6
      }
    ]
  }
}
```

### Exporting from Figma

1. In Figma, select your design
2. Go to **File > Export**
3. Choose **JSON** format
4. Click **Export**
5. Upload the downloaded file to the app

### Common Issues & Solutions

**"Invalid JSON file" error**:
- Ensure you exported as JSON from Figma
- Check file doesn't contain syntax errors

**Code generation fails**:
- Verify your Figma data has a `document` object with `children`
- Try with a simpler design first

**Performance scores low**:
- Run `npm run lighthouse:dev` for detailed report
- Check your network connection
- Close other browser tabs during testing

### Need Help?

- **Documentation**: See [README.md](./README.md) for detailed guide
- **Development**: See [DEVELOPMENT.md](./DEVELOPMENT.md) for setup instructions
- **Performance**: See [LIGHTHOUSE_CHECKLIST.md](./LIGHTHOUSE_CHECKLIST.md) for optimization

### Next Steps

1. **Try different stacks** to compare output
2. **Upload complex designs** with nested components
3. **Check the History** to reuse previous conversions
4. **Run performance tests** to verify optimization

Happy coding! 🎨➡️💻