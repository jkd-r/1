import express from 'express'
import cors from 'cors'
import { figmaToCodeRouter } from './src/routes/figma-to-code.js'
import { healthRouter } from './src/routes/health.js'

const app = express()
const PORT = process.env.PORT || 3000

// Middleware
app.use(cors())
app.use(express.json({ limit: '10mb' }))
app.use(express.static('dist'))

// Routes
app.use('/api/health', healthRouter)
app.use('/api/figma-to-code', figmaToCodeRouter)

// Serve the frontend
app.get('*', (req, res) => {
  res.sendFile('index.html', { root: 'dist' })
})

app.listen(PORT, () => {
  console.log(`🚀 Figma to Code Generator running on port ${PORT}`)
})