# Protocol EMR - Concept Brief (v2)

## Narrative Premise

Protocol EMR places players in the role of a research facility employee who awakens to find the building in lockdown mode. An automated emergency response system has been activated following an unspecified containment breach, and the player must navigate through the facility's increasingly complex security layers to uncover what happened and escape.

The facility, a cutting-edge biomedical research complex, was conducting experimental neural interface research when something went wrong. The player discovers they have limited access to facility systems through a prototype neural interface, which allows them to receive guidance from the facility's AI narrator while gradually unlocking more systems and information about the true nature of the research.

As players progress, they uncover layers of corporate conspiracy, scientific ethics violations, and the realization that they may be part of the experiment themselves. The narrative unfolds through environmental storytelling, recovered data logs, and the AI narrator's increasingly revealing commentary.

## Target Platform

- **Platform**: PC (Windows, macOS, Linux)
- **Engine**: Unity 2022.3 LTS or newer
- **Target Resolution**: 1920x1080 (16:9) with support for 2560x1440
- **Performance Targets**:
  - 60 FPS minimum on mid-range gaming PCs (GTX 1660/RX 580 equivalent)
  - 30 FPS minimum on integrated graphics (Intel Iris Xe, AMD Radeon Vega)
  - Memory usage: 4GB RAM minimum, 8GB recommended
  - Storage: 2GB installed size for prototype

## Desired Atmosphere

The game creates a **high-tech escape room** atmosphere that balances:

- **Tension and Mystery**: Constant sense of unease through ambient audio, environmental storytelling, and the unknown nature of the threat
- **Realistic Technology**: Plausible near-future sci-fi setting with grounded technology and interfaces
- **Isolation and Discovery**: Players feel alone but guided, creating a compelling solitary exploration experience
- **Intellectual Challenge**: Puzzles and systems that require observation and critical thinking rather than reflexes

The visual aesthetic emphasizes clean, sterile environments contrasted with areas of chaos and damage. Lighting is used strategically to guide attention and create mood, with dynamic shadows and environmental effects enhancing the sense of presence.

## Player Fantasy

Players want to feel like:

- **Problem Solvers**: Using their intellect to overcome complex, interconnected systems
- **Investigators**: Piecing together clues and uncovering hidden truths
- **Survivors**: Outsmarting automated systems and navigating dangerous environments
- **Hackers**: Gaining access to restricted information and bending systems to their will
- **Discoverers**: Experiencing the satisfaction of unraveling a complex mystery

The core fantasy is about being the smartest person in the room when everything is automated and hostile - using human intuition and creativity to overcome systems designed to contain and control.

## Reference Inspirations

### Games
- **Portal**: Environmental storytelling, AI narrator relationship, puzzle progression
- **The Talos Principle**: Philosophical narrative, puzzle variety, environmental exploration
- **SOMA**: Atmospheric tension, sci-fi horror themes, narrative discovery
- **The Witness**: Non-linear exploration, environmental puzzles, player-driven discovery
- **Control**: Modern office environment with supernatural elements, system-based gameplay

### Media
- **2001: A Space Odyssey**: AI relationship dynamics, clean aesthetic isolation
- **Ex Machina**: Modern AI ethics, contained environments, psychological tension
- **Black Mirror**: Near-future technology commentary, plausible dystopia
- **Arrival**: Communication and understanding as core mechanics

## Gameplay Loop

The core gameplay loop follows a **5-stage cycle**:

1. **Discover**: Enter a new area and identify available systems, terminals, and environmental clues
2. **Analyze**: Use the neural interface to scan and understand systems, receive AI guidance, and identify objectives
3. **Solve**: Engage with puzzles, hack terminals, manipulate environmental systems, and overcome obstacles
4. **Progress**: Unlock new areas, gain access to additional systems, and acquire new information
5. **Reflect**: Process narrative revelations through AI commentary and environmental storytelling

Each loop builds upon previous discoveries, with systems becoming increasingly complex and interconnected. The AI narrator provides context and hints while maintaining narrative mystery.

## 10 Workstreams

### 1. Setup
**Goal**: Establish development environment and core project structure
- Unity project setup with version control
- Build pipeline and CI/CD configuration
- Team collaboration tools and documentation
- Core project architecture and folder structure
- Development hardware and software requirements

### 2. Core Systems
**Goal**: Implement fundamental gameplay mechanics
- Player movement and interaction systems
- Physics and collision detection
- Save/load functionality
- State management and event systems
- Debug and development tools

### 3. UI/HUD
**Goal**: Create intuitive interface for player interaction
- Neural interface visualization
- Health and status indicators
- Inventory and item management displays
- System access terminals and interfaces
- Accessibility options and settings

### 4. Inventory
**Goal**: Implement item management and usage systems
- Item pickup and storage mechanics
- Item categories and properties
- Crafting and combination systems
- Key item and progression item tracking
- Visual feedback for item interactions

### 5. Phone System
**Goal**: Develop communication and information delivery
- AI narrator messaging interface
- Character dialogue system
- Notification and alert management
- Message history and search functionality
- Visual and audio integration

### 6. NPC Framework
**Goal**: Create character interaction foundation
- Character data and behavior systems
- Dialogue tree implementation
- Character relationship tracking
- Animation and visual representation systems
- AI behavior and pathfinding

### 7. AI Narrator
**Goal**: Implement dynamic narrative guidance system
- Context-aware response generation
- Event-driven commentary triggers
- Voice-over integration and timing
- Personality and tone management
- Player choice consequence tracking

### 8. Missions
**Goal**: Design and implement quest progression systems
- Mission structure and objective tracking
- Branching narrative logic
- Progress validation and completion detection
- Reward and consequence systems
- Mission editor tools for content creation

### 9. Procedural Generation
**Goal**: Create dynamic content and replayability
- Randomized puzzle elements
- Environmental variation systems
- Dynamic event triggers
- Procedural audio generation
- Content balancing and difficulty scaling
- End-to-end procedural story generation with seed-based determinism
- AI narrator coupling with dynamic narrative adaptation
- Room and encounter layering with story integration
- See **[Procedural Story Generation](./procedural-story-generation.md)** for detailed specifications

### 10. Audio & Polish
**Goal**: Achieve professional-quality presentation
- Audio integration and mixing
- Visual effects and shader implementation
- Performance optimization
- Quality assurance and bug fixing
- Final presentation and user experience refinement

## Build & Coding Roadmap

The development of Protocol EMR follows a structured roadmap that organizes the 10 workstreams into logical phases prioritizing gameplay-first implementation. Each development phase is designed to deliver functional, testable features within focused sprint cycles of 5 or fewer tasks.

**Development Phases:**

- **Foundation & Setup**: Project environment and core infrastructure
- **Core Systems**: Fundamental gameplay mechanics and player controls
- **Player Interface & Interaction**: UI, inventory, and item systems
- **Communication & NPC Systems**: AI narrator and character interactions
- **Narrative & Mission Systems**: Quest progression and branching story
- **Content Generation & Polish**: Procedural systems and final optimization

The roadmap emphasizes **gameplay first**, ensuring core player experience is established early and refined iteratively. Each phase builds upon previous systems with clear integration points and measurable milestones.

For a detailed breakdown of sprints, task distribution, and success criteria, see the **[Build & Coding Roadmap](./build-coding-roadmap.md)** document.

## Success Metrics

A successful prototype will achieve:

### Technical Metrics
- **Performance**: Consistent 60 FPS on target hardware
- **Stability**: No crashes during 2-hour play sessions
- **Load Times**: Initial load under 30 seconds, area transitions under 5 seconds
- **Memory Usage**: Peak memory usage under 4GB

### Gameplay Metrics
- **Completion Rate**: 70% of playtesters complete the core story
- **Engagement Time**: Average 2-3 hours for prototype completion
- **Puzzle Success Rate**: 80% of puzzles solved without external help
- **Player Retention**: 60% of players return for additional sessions

### Quality Metrics
- **Bug Count**: Zero critical bugs, fewer than 10 minor bugs
- **User Satisfaction**: 8/10 average rating from playtesters
- **Narrative Comprehension**: 90% of players understand core story elements
- **Interface Usability**: 85% of players find interfaces intuitive

## Assumptions

### Technical Assumptions
- Unity development team has intermediate to advanced experience
- Source control and project management tools are available
- Target hardware specifications remain stable during development
- Third-party assets and libraries will be available and properly licensed

### Design Assumptions
- Players are familiar with first-person exploration games
- Puzzle difficulty can be balanced through iterative testing
- AI narrator guidance will enhance rather than frustrate the experience
- Environmental storytelling will effectively convey narrative elements

### Resource Assumptions
- Voice acting can be sourced within budget constraints
- Audio assets can be acquired or created to quality standards
- Development timeline allows for adequate testing and iteration
- Team size and composition remain stable throughout development

## Open Questions

### Narrative Questions
- What is the specific nature of the research being conducted?
- How much of the facility's history should be revealed versus left mysterious?
- Should the AI narrator be trustworthy, deceptive, or evolving in reliability?
- What is the ultimate fate of the player character?

### Gameplay Questions
- What is the optimal balance between puzzle difficulty and player guidance?
- Should failure states include permanent consequences or allow retries?
- How much freedom should players have in exploration versus linear progression?
- What types of puzzles best serve the narrative and atmosphere?

### Technical Questions
- Which audio middleware (FMOD, Wwise, or Unity native) best serves project needs?
- What level of procedural generation adds value without compromising quality?
- How can we efficiently test and balance puzzle difficulty across different player types?
- What is the minimum viable feature set for a compelling prototype?

## Risks

### High-Risk Items
- **AI Narrator Implementation**: Complex system requiring sophisticated natural language processing and context awareness
- **Puzzle Balance**: Risk of puzzles being too difficult (frustrating) or too easy (boring)
- **Performance Optimization**: Complex systems may impact frame rates on target hardware
- **Audio Asset Acquisition**: Quality voice acting and sound effects may exceed budget or timeline

### Medium-Risk Items
- **Scope Creep**: Feature additions during development may impact timeline and quality
- **Technical Debt**: Rapid prototyping may create maintenance issues
- **Player Testing**: Limited access to diverse playtesters may skew feedback
- **Third-Party Dependencies**: External assets or libraries may have licensing or technical issues

### Mitigation Strategies
- **Early Prototyping**: Implement core systems early to identify technical challenges
- **Iterative Testing**: Regular playtesting with diverse user groups
- **Modular Design**: Create flexible systems that can adapt to changing requirements
- **Contingency Planning**: Budget and timeline buffers for high-risk elements
- **Documentation**: Comprehensive documentation to support team collaboration and knowledge transfer