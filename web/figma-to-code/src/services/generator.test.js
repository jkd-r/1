import { FigmaToCodeGenerator } from '../src/services/generator.js'

// Initialize generator
const generator = new FigmaToCodeGenerator()

// Sample Figma data for testing
const sampleFigmaData = {
  document: {
    id: '0:0',
    name: 'Test Frame',
    type: 'FRAME',
    children: [
      {
        id: '1:1',
        name: 'Test Button',
        type: 'RECTANGLE',
        visible: true,
        absoluteBoundingBox: {
          x: 100,
          y: 100,
          width: 200,
          height: 50
        },
        fills: [
          {
            type: 'SOLID',
            visible: true,
            opacity: 1,
            color: {
              r: 0.2,
              g: 0.4,
              b: 0.8
            }
          }
        ],
        cornerRadius: 8,
        children: []
      },
      {
        id: '1:2',
        name: 'Test Card',
        type: 'FRAME',
        visible: true,
        absoluteBoundingBox: {
          x: 50,
          y: 200,
          width: 300,
          height: 200
        },
        fills: [
          {
            type: 'SOLID',
            visible: true,
            opacity: 1,
            color: {
              r: 1,
              g: 1,
              b: 1
            }
          }
        ],
        strokes: [
          {
            type: 'SOLID',
            visible: true,
            opacity: 1,
            color: {
              r: 0.8,
              g: 0.8,
              b: 0.8
            }
          }
        ],
        cornerRadius: 12,
        opacity: 0.9,
        children: [
          {
            id: '2:1',
            name: 'Card Title',
            type: 'TEXT',
            visible: true,
            absoluteBoundingBox: {
              x: 70,
              y: 220,
              width: 100,
              height: 30
            },
            fills: [
              {
                type: 'SOLID',
                visible: true,
                opacity: 1,
                color: {
                  r: 0,
                  g: 0,
                  b: 0
                }
              }
            ],
            children: []
          }
        ]
      }
    ]
  }
}

describe('FigmaToCodeGenerator', () => {
  beforeEach(() => {
    // Clear history before each test
    generator.clearHistory()
  })

  describe('Constructor and Initialization', () => {
    test('should initialize with default values', () => {
      expect(generator.currentStack).toBe('react')
      expect(generator.currentFigmaData).toBeNull()
      expect(generator.history).toEqual([])
      expect(generator.conversionCount).toBe(0)
    })
  })

  describe('Stack Support', () => {
    test('should return all supported stacks', () => {
      const stacks = generator.getSupportedStacks()
      expect(stacks).toContain('react')
      expect(stacks).toContain('vue')
      expect(stacks).toContain('angular')
      expect(stacks).toContain('html')
      expect(stacks).toContain('tailwind')
      expect(stacks).toContain('bootstrap')
      expect(stacks).toHaveLength(6)
    })
  })

  describe('Code Generation', () => {
    test('should generate React code successfully', () => {
      const result = generator.generateCode(sampleFigmaData, 'react')
      
      expect(result.success).toBe(true)
      expect(result.code).toContain('import React')
      expect(result.code).toContain('const FigmaComponent')
      expect(result.code).toContain('export default FigmaComponent')
      expect(result.stack).toBe('react')
      expect(result.generationTime).toBeDefined()
    })

    test('should generate Vue code successfully', () => {
      const result = generator.generateCode(sampleFigmaData, 'vue')
      
      expect(result.success).toBe(true)
      expect(result.code).toContain('<template>')
      expect(result.code).toContain('<script setup>')
      expect(result.code).toContain('<style scoped>')
      expect(result.stack).toBe('vue')
    })

    test('should generate Angular code successfully', () => {
      const result = generator.generateCode(sampleFigmaData, 'angular')
      
      expect(result.success).toBe(true)
      expect(result.code).toContain('import { Component }')
      expect(result.code).toContain('@Component')
      expect(result.code).toContain('export class FigmaComponent')
      expect(result.stack).toBe('angular')
    })

    test('should generate HTML code successfully', () => {
      const result = generator.generateCode(sampleFigmaData, 'html')
      
      expect(result.success).toBe(true)
      expect(result.code).toContain('<!DOCTYPE html>')
      expect(result.code).toContain('<html lang="en">')
      expect(result.code).toContain('<head>')
      expect(result.code).toContain('<body>')
      expect(result.stack).toBe('html')
    })

    test('should generate Tailwind CSS code successfully', () => {
      const result = generator.generateCode(sampleFigmaData, 'tailwind')
      
      expect(result.success).toBe(true)
      expect(result.code).toContain('import React')
      expect(result.code).toContain('const FigmaComponent')
      expect(result.stack).toBe('tailwind')
    })

    test('should generate Bootstrap code successfully', () => {
      const result = generator.generateCode(sampleFigmaData, 'bootstrap')
      
      expect(result.success).toBe(true)
      expect(result.code).toContain('import React')
      expect(result.code).toContain("import 'bootstrap/dist/css/bootstrap.min.css'")
      expect(result.code).toContain('className="container"')
      expect(result.stack).toBe('bootstrap')
    })

    test('should handle unsupported stack', () => {
      const result = generator.generateCode(sampleFigmaData, 'unsupported')
      
      expect(result.success).toBe(false)
      expect(result.error).toContain('Unsupported stack: unsupported')
    })

    test('should handle invalid Figma data', () => {
      const result = generator.generateCode(null, 'react')
      
      expect(result.success).toBe(false)
      expect(result.error).toBeDefined()
    })

    test('should return non-empty code for all supported stacks', () => {
      const supportedStacks = generator.getSupportedStacks()
      
      supportedStacks.forEach(stack => {
        const result = generator.generateCode(sampleFigmaData, stack)
        expect(result.success).toBe(true)
        expect(result.code).toBeTruthy()
        expect(result.code.length).toBeGreaterThan(0)
      })
    })

    test('should generate valid code snippets', () => {
      const result = generator.generateCode(sampleFigmaData, 'react')
      
      expect(result.success).toBe(true)
      
      // Check for basic React component structure
      expect(result.code).toMatch(/import\s+React/)
      expect(result.code).toMatch(/const\s+\w+\s*=/)
      expect(result.code).match(/export\s+default/)
      
      // Check for JSX-like structure
      expect(result.code).toMatch(/<div/)
      expect(result.code).match(/<\/div>/)
    })
  })

  describe('Component Processing', () => {
    test('should extract components correctly', () => {
      const components = generator.extractComponents(sampleFigmaData)
      
      expect(components).toHaveLength(2)
      expect(components[0].name).toBe('Test Button')
      expect(components[0].type).toBe('RECTANGLE')
      expect(components[1].name).toBe('Test Card')
      expect(components[1].type).toBe('FRAME')
    })

    test('should process node properties correctly', () => {
      const components = generator.extractComponents(sampleFigmaData)
      const button = components[0]
      
      expect(button.width).toBe(200)
      expect(button.height).toBe(50)
      expect(button.cornerRadius).toBe(8)
      expect(button.backgroundColor).toBe('rgba(51, 102, 204, 1)')
    })

    test('should handle nested components', () => {
      const components = generator.extractComponents(sampleFigmaData)
      const card = components[1]
      
      expect(card.children).toHaveLength(1)
      expect(card.children[0].name).toBe('Card Title')
      expect(card.children[0].type).toBe('TEXT')
    })
  })

  describe('Color Extraction', () => {
    test('should extract background colors correctly', () => {
      const fills = [
        {
          type: 'SOLID',
          visible: true,
          opacity: 0.5,
          color: { r: 1, g: 0, b: 0 }
        }
      ]
      
      const color = generator.extractBackgroundColor(fills)
      expect(color).toBe('rgba(255, 0, 0, 0.5)')
    })

    test('should extract border colors correctly', () => {
      const strokes = [
        {
          type: 'SOLID',
          visible: true,
          opacity: 1,
          color: { r: 0, g: 1, b: 0 }
        }
      ]
      
      const color = generator.extractBorderColor(strokes)
      expect(color).toBe('rgba(0, 255, 0, 1)')
    })

    test('should return transparent for missing colors', () => {
      const color = generator.extractBackgroundColor([])
      expect(color).toBe('transparent')
    })
  })

  describe('Style Generation', () => {
    test('should generate inline styles correctly', () => {
      const component = {
        backgroundColor: 'rgba(255, 0, 0, 1)',
        width: 100,
        height: 50,
        borderRadius: 8
      }
      
      const styles = generator.generateInlineStyles(component)
      const parsedStyles = JSON.parse(styles)
      
      expect(parsedStyles.backgroundColor).toBe('rgba(255, 0, 0, 1)')
      expect(parsedStyles.width).toBe('100px')
      expect(parsedStyles.height).toBe('50px')
      expect(parsedStyles.borderRadius).toBe('8px')
    })

    test('should generate CSS properties correctly', () => {
      const component = {
        backgroundColor: 'rgba(255, 0, 0, 1)',
        borderColor: 'rgba(0, 255, 0, 1)',
        borderRadius: 12,
        width: 200,
        height: 100,
        opacity: 0.8
      }
      
      const css = generator.generateCSSProperties(component)
      
      expect(css).toContain('background-color: rgba(255, 0, 0, 1)')
      expect(css).toContain('border: 1px solid rgba(0, 255, 0, 1)')
      expect(css).toContain('border-radius: 12px')
      expect(css).toContain('width: 200px')
      expect(css).toContain('height: 100px')
      expect(css).toContain('opacity: 0.8')
    })
  })

  describe('History Management', () => {
    test('should save conversion to history', () => {
      const initialHistoryCount = generator.getHistory().length
      
      generator.generateCode(sampleFigmaData, 'react')
      
      const history = generator.getHistory()
      expect(history).toHaveLength(initialHistoryCount + 1)
      expect(history[0]).toHaveProperty('id')
      expect(history[0]).toHaveProperty('timestamp')
      expect(history[0]).toHaveProperty('stack', 'react')
      expect(history[0]).toHaveProperty('code')
      expect(history[0]).toHaveProperty('generationTime')
    })

    test('should clear history correctly', () => {
      generator.generateCode(sampleFigmaData, 'react')
      expect(generator.getHistory().length).toBeGreaterThan(0)
      
      generator.clearHistory()
      expect(generator.getHistory()).toHaveLength(0)
    })

    test('should limit history to 50 items', () => {
      // Add more than 50 items
      for (let i = 0; i < 60; i++) {
        generator.generateCode(sampleFigmaData, 'react')
      }
      
      const history = generator.getHistory()
      expect(history).toHaveLength(50)
    })
  })

  describe('Data Validation', () => {
    test('should validate Figma data correctly', () => {
      expect(() => generator.validateFigmaData(sampleFigmaData)).not.toThrow()
      
      expect(() => generator.validateFigmaData(null)).toThrow('No Figma data provided')
      expect(() => generator.validateFigmaData({})).toThrow('Invalid Figma data: missing document')
      expect(() => generator.validateFigmaData({ document: {} })).toThrow('Invalid Figma data: no children found')
    })

    test('should sanitize Figma data correctly', () => {
      const figmaDataWithImage = {
        document: {
          children: [
            {
              type: 'IMAGE',
              imageRef: 'some-ref',
              imageHash: 'some-hash',
              children: []
            }
          ]
        }
      }
      
      const sanitized = generator.sanitizeFigmaData(figmaDataWithImage)
      
      expect(sanitized.document.children[0].imageRef).toBeUndefined()
      expect(sanitized.document.children[0].imageHash).toBeUndefined()
    })
  })

  describe('Statistics', () => {
    test('should return correct statistics', () => {
      generator.generateCode(sampleFigmaData, 'react')
      generator.generateCode(sampleFigmaData, 'vue')
      
      const stats = generator.getStats()
      
      expect(stats.conversionCount).toBe(2)
      expect(stats.historyCount).toBe(2)
      expect(stats.supportedStacks).toBe(6)
      expect(stats.averageTime).toBeDefined()
    })
  })

  describe('Error Handling', () => {
    test('should handle malformed Figma data gracefully', () => {
      const malformedData = {
        document: {
          children: [
            {
              // Missing required properties
              name: 'Broken Component'
            }
          ]
        }
      }
      
      const result = generator.generateCode(malformedData, 'react')
      expect(result.success).toBe(true) // Should still generate something
    })

    test('should handle empty children array', () => {
      const emptyData = {
        document: {
          children: []
        }
      }
      
      const result = generator.generateCode(emptyData, 'react')
      expect(result.success).toBe(true)
      expect(result.code).toContain('import React')
    })
  })
})