import { FigmaToCodeGenerator } from '../services/generator.js'

const generator = new FigmaToCodeGenerator()

export async function generateCode(req, res) {
  try {
    const { figmaData, stack } = req.body

    if (!figmaData || !stack) {
      return res.status(400).json({
        success: false,
        error: 'Missing required parameters: figmaData and stack'
      })
    }

    // Validate stack
    const supportedStacks = generator.getSupportedStacks()
    if (!supportedStacks.includes(stack)) {
      return res.status(400).json({
        success: false,
        error: `Unsupported stack: ${stack}. Supported stacks: ${supportedStacks.join(', ')}`
      })
    }

    // Generate code
    const result = generator.generateCode(figmaData, stack)

    if (result.success) {
      res.json({
        success: true,
        data: {
          code: result.code,
          stack: result.stack,
          generationTime: result.generationTime
        }
      })
    } else {
      res.status(500).json({
        success: false,
        error: result.error
      })
    }
  } catch (error) {
    console.error('Generation error:', error)
    res.status(500).json({
      success: false,
      error: 'Internal server error during code generation'
    })
  }
}

export async function getHistory(req, res) {
  try {
    const history = generator.getHistory()
    res.json({
      success: true,
      data: history
    })
  } catch (error) {
    console.error('History error:', error)
    res.status(500).json({
      success: false,
      error: 'Failed to retrieve history'
    })
  }
}

export async function clearHistory(req, res) {
  try {
    generator.clearHistory()
    res.json({
      success: true,
      message: 'History cleared successfully'
    })
  } catch (error) {
    console.error('Clear history error:', error)
    res.status(500).json({
      success: false,
      error: 'Failed to clear history'
    })
  }
}

export async function getStats(req, res) {
  try {
    const stats = generator.getStats()
    res.json({
      success: true,
      data: stats
    })
  } catch (error) {
    console.error('Stats error:', error)
    res.status(500).json({
      success: false,
      error: 'Failed to retrieve stats'
    })
  }
}

export async function getSupportedStacks(req, res) {
  try {
    const stacks = generator.getSupportedStacks()
    res.json({
      success: true,
      data: stacks
    })
  } catch (error) {
    console.error('Stacks error:', error)
    res.status(500).json({
      success: false,
      error: 'Failed to retrieve supported stacks'
    })
  }
}