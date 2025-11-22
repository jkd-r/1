import { FigmaToCodeGenerator } from './services/generator.js'

// DOM Elements
const uploadArea = document.getElementById('uploadArea')
const fileInput = document.getElementById('fileInput')
const generateBtn = document.getElementById('generateBtn')
const clearBtn = document.getElementById('clearBtn')
const loading = document.getElementById('loading')
const outputArea = document.getElementById('outputArea')
const previewFrame = document.getElementById('previewFrame')
const historyArea = document.getElementById('historyArea')
const conversionCountEl = document.getElementById('conversionCount')
const avgTimeEl = document.getElementById('avgTime')

// State
let currentFigmaData = null
let selectedStack = 'react'
let generator = new FigmaToCodeGenerator()

// Initialize
document.addEventListener('DOMContentLoaded', () => {
  initializeEventListeners()
  updateStats()
  loadHistory()
})

function initializeEventListeners() {
  // File upload
  uploadArea.addEventListener('click', () => fileInput.click())
  uploadArea.addEventListener('dragover', handleDragOver)
  uploadArea.addEventListener('dragleave', handleDragLeave)
  uploadArea.addEventListener('drop', handleDrop)
  fileInput.addEventListener('change', handleFileSelect)

  // Stack selection
  document.querySelectorAll('.stack-option').forEach(option => {
    option.addEventListener('click', () => selectStack(option.dataset.stack))
  })

  // Buttons
  generateBtn.addEventListener('click', generateCode)
  clearBtn.addEventListener('click', clearAll)

  // Tabs
  document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => switchTab(tab.dataset.tab))
  })
}

function handleDragOver(e) {
  e.preventDefault()
  uploadArea.classList.add('dragover')
}

function handleDragLeave(e) {
  e.preventDefault()
  uploadArea.classList.remove('dragover')
}

function handleDrop(e) {
  e.preventDefault()
  uploadArea.classList.remove('dragover')
  
  const files = e.dataTransfer.files
  if (files.length > 0) {
    handleFile(files[0])
  }
}

function handleFileSelect(e) {
  const files = e.target.files
  if (files.length > 0) {
    handleFile(files[0])
  }
}

function handleFile(file) {
  if (!file.type.includes('json')) {
    showNotification('Please upload a JSON file', 'error')
    return
  }

  const reader = new FileReader()
  reader.onload = (e) => {
    try {
      const figmaData = JSON.parse(e.target.result)
      currentFigmaData = figmaData
      showNotification('Figma data loaded successfully', 'success')
      uploadArea.innerHTML = `
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin: 0 auto 1rem;">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
          <polyline points="22 4 12 14.01 9 11.01"></polyline>
        </svg>
        <h3>${file.name}</h3>
        <p>Click to replace or drag new file</p>
      `
    } catch (error) {
      showNotification('Invalid JSON file', 'error')
    }
  }
  reader.readAsText(file)
}

function selectStack(stack) {
  selectedStack = stack
  
  // Update UI
  document.querySelectorAll('.stack-option').forEach(option => {
    option.classList.remove('selected')
  })
  document.querySelector(`[data-stack="${stack}"]`).classList.add('selected')
}

async function generateCode() {
  if (!currentFigmaData) {
    showNotification('Please upload Figma data first', 'error')
    return
  }

  // Show loading
  loading.classList.add('active')
  generateBtn.disabled = true

  try {
    const result = generator.generateCode(currentFigmaData, selectedStack)
    
    if (result.success) {
      displayCode(result.code)
      updatePreview(result.code)
      updateStats()
      loadHistory()
      showNotification(`Code generated for ${selectedStack} in ${result.generationTime}s`, 'success')
    } else {
      showNotification(`Generation failed: ${result.error}`, 'error')
    }
  } catch (error) {
    showNotification('Generation failed: ' + error.message, 'error')
  } finally {
    loading.classList.remove('active')
    generateBtn.disabled = false
  }
}

function displayCode(code) {
  outputArea.innerHTML = `<pre>${escapeHtml(code)}</pre>`
}

function updatePreview(code) {
  // Create a simple preview for HTML code
  if (selectedStack === 'html') {
    previewFrame.srcdoc = code
  } else {
    // For other frameworks, show a message
    previewFrame.srcdoc = `
      <html>
        <head>
          <style>
            body { 
              font-family: Arial, sans-serif; 
              padding: 20px; 
              text-align: center; 
              background: #f5f5f5;
            }
            .message {
              background: white;
              padding: 20px;
              border-radius: 8px;
              box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            }
          </style>
        </head>
        <body>
          <div class="message">
            <h3>Preview Available for HTML Only</h3>
            <p>For ${selectedStack}, please copy the generated code and run it in your development environment.</p>
          </div>
        </body>
      </html>
    `
  }
}

function clearAll() {
  currentFigmaData = null
  selectedStack = 'react'
  
  // Reset UI
  uploadArea.innerHTML = `
    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin: 0 auto 1rem;">
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
      <polyline points="7 10 12 15 17 10"></polyline>
      <line x1="12" y1="15" x2="12" y2="3"></line>
    </svg>
    <h3>Drop your Figma JSON here</h3>
    <p>or click to browse</p>
    <input type="file" id="fileInput" accept=".json" style="display: none;">
  `
  
  outputArea.innerHTML = '<pre>// Your generated code will appear here...</pre>'
  previewFrame.srcdoc = ''
  
  // Reset stack selection
  document.querySelectorAll('.stack-option').forEach(option => {
    option.classList.remove('selected')
  })
  document.querySelector('[data-stack="react"]').classList.add('selected')
  
  // Re-initialize file input
  const newFileInput = document.createElement('input')
  newFileInput.type = 'file'
  newFileInput.id = 'fileInput'
  newFileInput.accept = '.json'
  newFileInput.style.display = 'none'
  uploadArea.appendChild(newFileInput)
  newFileInput.addEventListener('change', handleFileSelect)
  
  showNotification('All data cleared', 'info')
}

function switchTab(tabName) {
  // Update tab buttons
  document.querySelectorAll('.tab').forEach(tab => {
    tab.classList.remove('active')
  })
  document.querySelector(`[data-tab="${tabName}"]`).classList.add('active')

  // Update tab content
  document.querySelectorAll('.tab-content').forEach(content => {
    content.style.display = 'none'
  })
  
  if (tabName === 'output') {
    document.getElementById('outputTab').style.display = 'block'
  } else if (tabName === 'preview') {
    document.getElementById('previewTab').style.display = 'block'
  } else if (tabName === 'history') {
    document.getElementById('historyTab').style.display = 'block'
  }
}

function loadHistory() {
  const history = generator.getHistory()
  
  if (history.length === 0) {
    historyArea.innerHTML = '<p style="color: var(--gray-500); text-align: center;">No previous conversions yet</p>'
    return
  }

  historyArea.innerHTML = history.map(item => `
    <div class="history-item" onclick="loadHistoryItem('${item.id}')">
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <div>
          <strong>${item.stack.toUpperCase()}</strong>
          <span style="color: var(--gray-500); margin-left: 8px;">${new Date(item.timestamp).toLocaleString()}</span>
        </div>
        <div style="color: var(--gray-500);">
          ${item.generationTime}s
        </div>
      </div>
    </div>
  `).join('')
}

function loadHistoryItem(itemId) {
  const history = generator.getHistory()
  const item = history.find(h => h.id.toString() === itemId)
  
  if (item) {
    currentFigmaData = item.figmaData
    selectedStack = item.stack
    displayCode(item.code)
    updatePreview(item.code)
    selectStack(item.stack)
    switchTab('output')
    showNotification('History item loaded', 'success')
  }
}

function updateStats() {
  const stats = generator.getStats()
  conversionCountEl.textContent = stats.conversionCount
  avgTimeEl.textContent = stats.averageTime + 's'
}

function showNotification(message, type = 'info') {
  // Create notification element
  const notification = document.createElement('div')
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 12px 20px;
    border-radius: 8px;
    color: white;
    font-weight: 500;
    z-index: 1000;
    transform: translateX(100%);
    transition: transform 0.3s ease;
  `

  // Set color based on type
  const colors = {
    success: '#10b981',
    error: '#ef4444',
    info: '#6366f1',
    warning: '#f59e0b'
  }
  
  notification.style.backgroundColor = colors[type] || colors.info
  notification.textContent = message

  document.body.appendChild(notification)

  // Animate in
  setTimeout(() => {
    notification.style.transform = 'translateX(0)'
  }, 100)

  // Remove after 3 seconds
  setTimeout(() => {
    notification.style.transform = 'translateX(100%)'
    setTimeout(() => {
      document.body.removeChild(notification)
    }, 300)
  }, 3000)
}

function escapeHtml(text) {
  const div = document.createElement('div')
  div.textContent = text
  return div.innerHTML
}

// Make loadHistoryItem available globally
window.loadHistoryItem = loadHistoryItem