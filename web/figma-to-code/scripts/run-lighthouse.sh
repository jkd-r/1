# Performance Testing Script

# Check if Lighthouse CLI is installed
if ! command -v lighthouse &> /dev/null; then
    echo "Installing Lighthouse CLI..."
    npm install -g lighthouse
fi

# Check if the development server is running
if ! curl -s http://localhost:5173 > /dev/null; then
    echo "Starting development server..."
    npm run dev &
    sleep 10  # Wait for server to start
fi

# Run Lighthouse audit
echo "Running Lighthouse performance audit..."
lighthouse http://localhost:5173 \
    --output=html \
    --output-path=./lighthouse-report.html \
    --chrome-flags="--headless" \
    --quiet

# Check if report was generated
if [ -f "./lighthouse-report.html" ]; then
    echo "✅ Lighthouse report generated: lighthouse-report.html"
    echo "📊 Open the report in your browser to view detailed performance metrics"
else
    echo "❌ Failed to generate Lighthouse report"
    exit 1
fi