class FigmaToCodeGenerator {
  constructor() {
    this.history = this.loadHistory()
    this.currentStack = 'react'
    this.currentFigmaData = null
    this.conversionCount = parseInt(localStorage.getItem('conversionCount') || '0')
  }

  // Main generation method
  generateCode(figmaData, stack) {
    const startTime = performance.now()
    
    try {
      const code = this.convertToStack(figmaData, stack)
      const endTime = performance.now()
      const generationTime = ((endTime - startTime) / 1000).toFixed(2)

      // Save to history
      const historyItem = {
        id: Date.now(),
        timestamp: new Date().toISOString(),
        stack,
        figmaData: this.sanitizeFigmaData(figmaData),
        code,
        generationTime
      }

      this.addToHistory(historyItem)
      this.conversionCount++
      localStorage.setItem('conversionCount', this.conversionCount.toString())

      return {
        success: true,
        code,
        generationTime,
        stack
      }
    } catch (error) {
      return {
        success: false,
        error: error.message,
        stack
      }
    }
  }

  // Convert Figma data to specific stack
  convertToStack(figmaData, stack) {
    switch (stack) {
      case 'react':
        return this.generateReactCode(figmaData)
      case 'vue':
        return this.generateVueCode(figmaData)
      case 'angular':
        return this.generateAngularCode(figmaData)
      case 'html':
        return this.generateHTMLCode(figmaData)
      case 'tailwind':
        return this.generateTailwindCode(figmaData)
      case 'bootstrap':
        return this.generateBootstrapCode(figmaData)
      default:
        throw new Error(`Unsupported stack: ${stack}`)
    }
  }

  // React component generation
  generateReactCode(figmaData) {
    const components = this.extractComponents(figmaData)
    const imports = new Set()

    let jsx = components.map(comp => {
      const styles = this.generateInlineStyles(comp)
      const props = this.generateProps(comp)
      
      if (comp.children && comp.children.length > 0) {
        imports.add('useState')
        return `<div${props} style={${styles}}>
${comp.children.map(child => `  ${this.generateReactCode({ document: { children: [child] } })}`).join('\n')}
</div>`
      }
      
      return `<div${props} style={${styles}}>${comp.name || ''}</div>`
    }).join('\n')

    const importStatements = Array.from(imports).length > 0 
      ? `import React, { ${Array.from(imports).join(', ')} } from 'react';\n\n`
      : `import React from 'react';\n\n`

    return `${importStatements}const FigmaComponent = () => {
  return (
    ${jsx}
  );
};

export default FigmaComponent;`
  }

  // Vue component generation
  generateVueCode(figmaData) {
    const components = this.extractComponents(figmaData)
    const template = this.generateVueTemplate(components)
    const script = this.generateVueScript(components)
    const style = this.generateVueStyle(components)

    return `<template>
${template}
</template>

<script setup>
${script}
</script>

<style scoped>
${style}
</style>`
  }

  // Angular component generation
  generateAngularCode(figmaData) {
    const components = this.extractComponents(figmaData)
    const template = this.generateAngularTemplate(components)
    const componentClass = this.generateAngularComponent(components)

    return `import { Component } from '@angular/core';

@Component({
  selector: 'app-figma-component',
  template: \`
${template}
  \`,
  styles: [\`
${this.generateAngularStyles(components)}
  \`]
})
export class FigmaComponent {
${componentClass}
}`
  }

  // HTML generation
  generateHTMLCode(figmaData) {
    const components = this.extractComponents(figmaData)
    const html = this.generateHTMLStructure(components)
    const css = this.generateCSS(components)

    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Figma Design</title>
    <style>
${css}
    </style>
</head>
<body>
${html}
</body>
</html>`
  }

  // Tailwind CSS generation
  generateTailwindCode(figmaData) {
    const components = this.extractComponents(figmaData)
    const jsx = this.generateTailwindJSX(components)

    return `import React from 'react';

const FigmaComponent = () => {
  return (
    ${jsx}
  );
};

export default FigmaComponent;`
  }

  // Bootstrap generation
  generateBootstrapCode(figmaData) {
    const components = this.extractComponents(figmaData)
    const jsx = this.generateBootstrapJSX(components)

    return `import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';

const FigmaComponent = () => {
  return (
    <div className="container">
${jsx}
    </div>
  );
};

export default FigmaComponent;`
  }

  // Helper methods
  extractComponents(figmaData) {
    if (!figmaData || !figmaData.document || !figmaData.document.children) {
      return []
    }
    
    return figmaData.document.children.map(node => this.processNode(node))
  }

  processNode(node) {
    const component = {
      id: node.id,
      name: node.name,
      type: node.type,
      visible: node.visible !== false,
      children: []
    }

    // Extract position and size
    if (node.absoluteBoundingBox) {
      component.x = node.absoluteBoundingBox.x
      component.y = node.absoluteBoundingBox.y
      component.width = node.absoluteBoundingBox.width
      component.height = node.absoluteBoundingBox.height
    }

    // Extract styling
    if (node.fills) {
      component.backgroundColor = this.extractBackgroundColor(node.fills)
    }

    if (node.strokes) {
      component.borderColor = this.extractBorderColor(node.strokes)
    }

    if (node.cornerRadius !== undefined) {
      component.borderRadius = node.cornerRadius
    }

    if (node.opacity !== undefined) {
      component.opacity = node.opacity
    }

    // Process children
    if (node.children) {
      component.children = node.children.map(child => this.processNode(child))
    }

    return component
  }

  extractBackgroundColor(fills) {
    const visibleFill = fills.find(fill => fill.visible !== false && fill.type === 'SOLID')
    if (visibleFill && visibleFill.color) {
      const { r, g, b } = visibleFill.color
      return `rgba(${Math.round(r * 255)}, ${Math.round(g * 255)}, ${Math.round(b * 255)}, ${visibleFill.opacity || 1})`
    }
    return 'transparent'
  }

  extractBorderColor(strokes) {
    const visibleStroke = strokes.find(stroke => stroke.visible !== false && stroke.type === 'SOLID')
    if (visibleStroke && visibleStroke.color) {
      const { r, g, b } = visibleStroke.color
      return `rgba(${Math.round(r * 255)}, ${Math.round(g * 255)}, ${Math.round(b * 255)}, ${visibleStroke.opacity || 1})`
    }
    return 'transparent'
  }

  generateInlineStyles(component) {
    const styles = {}
    
    if (component.backgroundColor) {
      styles.backgroundColor = component.backgroundColor
    }
    
    if (component.borderColor) {
      styles.border = `1px solid ${component.borderColor}`
    }
    
    if (component.borderRadius) {
      styles.borderRadius = `${component.borderRadius}px`
    }
    
    if (component.width) {
      styles.width = `${component.width}px`
    }
    
    if (component.height) {
      styles.height = `${component.height}px`
    }
    
    if (component.opacity !== undefined) {
      styles.opacity = component.opacity
    }

    return JSON.stringify(styles)
  }

  generateProps(component) {
    const props = []
    
    if (component.name) {
      props.push(`title="${component.name}"`)
    }
    
    return props.length > 0 ? ` ${props.join(' ')}` : ''
  }

  generateVueTemplate(components) {
    return components.map(comp => {
      const styles = this.generateVueStyleBindings(comp)
      return `<div${styles}>${comp.name || ''}</div>`
    }).join('\n  ')
  }

  generateVueScript(components) {
    // Add reactive data if needed
    return `// Vue 3 Composition API
// Add your reactive data and methods here`
  }

  generateVueStyle(components) {
    return components.map(comp => {
      const selector = comp.name ? `[title="${comp.name}"]` : '.component'
      return `${selector} {
${this.generateCSSProperties(comp)}
}`
    }).join('\n')
  }

  generateAngularTemplate(components) {
    return components.map(comp => {
      const styles = this.generateAngularStyleBindings(comp)
      return `<div${styles}>${comp.name || ''}</div>`
    }).join('\n    ')
  }

  generateAngularComponent(components) {
    return `// Angular component logic
// Add your properties and methods here`
  }

  generateAngularStyles(components) {
    return components.map(comp => {
      return `${this.generateCSSProperties(comp)}`
    }).join('\n')
  }

  generateHTMLStructure(components) {
    return components.map(comp => {
      const styles = this.generateHTMLInlineStyles(comp)
      return `  <div${styles}>${comp.name || ''}</div>`
    }).join('\n')
  }

  generateCSS(components) {
    return components.map(comp => {
      const selector = comp.name ? `#${comp.name.replace(/\s+/g, '-').toLowerCase()}` : '.component'
      return `${selector} {
${this.generateCSSProperties(comp)}
}`
    }).join('\n')
  }

  generateTailwindJSX(components) {
    return components.map(comp => {
      const classes = this.generateTailwindClasses(comp)
      return `<div className="${classes}">${comp.name || ''}</div>`
    }).join('\n    ')
  }

  generateBootstrapJSX(components) {
    return components.map(comp => {
      const classes = this.generateBootstrapClasses(comp)
      return `<div className="${classes}">${comp.name || ''}</div>`
    }).join('\n      ')
  }

  generateTailwindClasses(component) {
    const classes = []
    
    // Background colors
    if (component.backgroundColor) {
      classes.push('bg-gray-100')
    }
    
    // Border radius
    if (component.borderRadius) {
      if (component.borderRadius >= 8) classes.push('rounded-lg')
      else classes.push('rounded')
    }
    
    // Spacing
    classes.push('p-4', 'm-2')
    
    return classes.join(' ')
  }

  generateBootstrapClasses(component) {
    const classes = []
    
    // Basic styling
    classes.push('card', 'p-3', 'mb-3')
    
    return classes.join(' ')
  }

  generateCSSProperties(component) {
    const props = []
    
    if (component.backgroundColor) {
      props.push(`  background-color: ${component.backgroundColor};`)
    }
    
    if (component.borderColor) {
      props.push(`  border: 1px solid ${component.borderColor};`)
    }
    
    if (component.borderRadius) {
      props.push(`  border-radius: ${component.borderRadius}px;`)
    }
    
    if (component.width) {
      props.push(`  width: ${component.width}px;`)
    }
    
    if (component.height) {
      props.push(`  height: ${component.height}px;`)
    }
    
    if (component.opacity !== undefined) {
      props.push(`  opacity: ${component.opacity};`)
    }
    
    return props.join('\n')
  }

  generateVueStyleBindings(component) {
    const styles = []
    
    if (component.backgroundColor) {
      styles.push(`:style="{ backgroundColor: '${component.backgroundColor}' }"`)
    }
    
    return styles.length > 0 ? ` ${styles.join(' ')}` : ''
  }

  generateAngularStyleBindings(component) {
    const styles = []
    
    if (component.backgroundColor) {
      styles.push(`[style.backgroundColor]="'${component.backgroundColor}'"`)
    }
    
    return styles.length > 0 ? ` ${styles.join(' ')}` : ''
  }

  generateHTMLInlineStyles(component) {
    const styles = []
    
    if (component.backgroundColor) {
      styles.push(`background-color: ${component.backgroundColor}`)
    }
    
    if (component.borderColor) {
      styles.push(`border: 1px solid ${component.borderColor}`)
    }
    
    if (component.borderRadius) {
      styles.push(`border-radius: ${component.borderRadius}px`)
    }
    
    if (component.width) {
      styles.push(`width: ${component.width}px`)
    }
    
    if (component.height) {
      styles.push(`height: ${component.height}px`)
    }
    
    return styles.length > 0 ? ` style="${styles.join('; ')}"` : ''
  }

  // History management
  loadHistory() {
    try {
      return JSON.parse(localStorage.getItem('figmaToCodeHistory') || '[]')
    } catch {
      return []
    }
  }

  saveHistory() {
    try {
      localStorage.setItem('figmaToCodeHistory', JSON.stringify(this.history))
    } catch (error) {
      console.warn('Failed to save history:', error)
    }
  }

  addToHistory(item) {
    this.history.unshift(item)
    // Keep only last 50 items
    if (this.history.length > 50) {
      this.history = this.history.slice(0, 50)
    }
    this.saveHistory()
  }

  clearHistory() {
    this.history = []
    this.saveHistory()
  }

  getHistory() {
    return this.history
  }

  // Utility methods
  sanitizeFigmaData(figmaData) {
    // Remove sensitive data and limit size
    const sanitized = JSON.parse(JSON.stringify(figmaData))
    
    // Remove images and large binary data
    const removeImages = (node) => {
      if (node.type === 'IMAGE') {
        delete node.imageRef
        delete node.imageHash
      }
      if (node.children) {
        node.children.forEach(removeImages)
      }
    }
    
    if (sanitized.document) {
      removeImages(sanitized.document)
    }
    
    return sanitized
  }

  validateFigmaData(figmaData) {
    if (!figmaData) {
      throw new Error('No Figma data provided')
    }
    
    if (!figmaData.document) {
      throw new Error('Invalid Figma data: missing document')
    }
    
    if (!figmaData.document.children) {
      throw new Error('Invalid Figma data: no children found')
    }
    
    return true
  }

  getSupportedStacks() {
    return ['react', 'vue', 'angular', 'html', 'tailwind', 'bootstrap']
  }

  getStats() {
    return {
      conversionCount: this.conversionCount,
      historyCount: this.history.length,
      supportedStacks: this.getSupportedStacks().length,
      averageTime: this.history.length > 0 
        ? (this.history.reduce((sum, item) => sum + parseFloat(item.generationTime), 0) / this.history.length).toFixed(2)
        : '0.0'
    }
  }
}

export { FigmaToCodeGenerator }