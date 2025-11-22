import { Router } from 'express'
import { generateCode, getHistory, clearHistory, getStats, getSupportedStacks } from '../controllers/figma-to-code.js'

const router = Router()

// Generate code from Figma data
router.post('/generate', generateCode)

// Get conversion history
router.get('/history', getHistory)

// Clear conversion history
router.delete('/history', clearHistory)

// Get usage statistics
router.get('/stats', getStats)

// Get supported stacks
router.get('/stacks', getSupportedStacks)

export { router as figmaToCodeRouter }