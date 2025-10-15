# Multi-Project Vertical Slice Architecture

How should features be organized across the three-layer project structure to maintain cohesion while preserving layer separation?

# Decision

We've chosen **vertical slice architecture with parallel folder structures** across projects, because this keeps feature code cohesive and discoverable while maintaining proper dependency flow between layers.

## Impact

- **Feature Cohesion**: Each feature's code is easy to locate across all layers
- **Discoverability**: Parallel folder structure makes navigation intuitive
- **Independent Evolution**: Features can be added, modified, or removed without affecting others
- **Reduced Coupling**: Features don't share code inappropriately
- **Clear Ownership**: Each feature slice has clear boundaries
- **Testability**: Feature tests are naturally organized alongside feature code

**Implementation Structure:**
```
Maui/Features/CurrentPrice/
├── CurrentPriceView.xaml          # UI (ContentView)
└── CurrentPriceView.xaml.cs

Maui.Core/Features/CurrentPrice/
├── CurrentPriceViewModel.cs       # Business logic
└── CurrentPriceOperations.cs      # Pure functions

Maui.Infrastructure/              # Shared infrastructure
├── Repositories/                  # Used by all features
└── Services/                      # Used by all features
```

# Solution Proposals

## 1. Layered Architecture (Technical Layers)

Organize code by technical layer (ViewModels/, Services/, Repositories/) with feature code scattered across these folders.

### Pros/Cons

**Pros:**
- Traditional architecture pattern
- Clear technical boundaries
- Easy to find all ViewModels/Services in one place

**Cons:**
- Feature code scattered across multiple folders
- Hard to see full scope of a feature
- Encourages shared code that couples features
- Difficult to remove features cleanly
- Navigation requires jumping between distant folders
- Features become tangled over time

## 2. Vertical Slice with Parallel Folders (Chosen)

Organize features in parallel folders across layers, with shared infrastructure at each layer level.

### Pros/Cons

**Pros:**
- Feature code is cohesive and discoverable
- Easy to find all parts of a feature
- Features evolve independently
- Clear boundaries between features
- Simple to add/remove features
- Parallel structure is intuitive
- Shared infrastructure explicitly separated

**Cons:**
- Must maintain parallel folder structure discipline
- Slightly more folders to navigate
- Need to decide what's "shared" vs "feature"
- Can feel redundant with many small features

## 3. Single Feature Folder with Subprojects

All feature code in one Maui/Features/ folder with technical subfolders (UI/, ViewModels/, etc.).

### Pros/Cons

**Pros:**
- All feature code in one location
- Very obvious feature boundaries

**Cons:**
- Breaks project layer separation
- Can't test ViewModels on Linux (mixed with MAUI UI)
- Violates three-layer architecture benefits
- Forces Windows runners for all tests
- Creates circular dependency risks

## 4. Vertical Slice with Nested Folders

Each feature is a complete vertical slice including infrastructure, business logic, and UI in nested folders.

### Pros/Cons

**Pros:**
- Complete feature isolation
- Everything for a feature in one place
- Very clear feature boundaries

**Cons:**
- Duplicates infrastructure code across features
- Harder to share common infrastructure
- Repository/service code per feature (not shared)
- API clients duplicated per feature
- Much more boilerplate
- Violates DRY principle for shared infrastructure
