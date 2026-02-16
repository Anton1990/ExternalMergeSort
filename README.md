# ExternalMergeSort

External merge sort implementation in C# (.NET 8) for sorting large text files that don't fit in memory.

## File Format

Each line follows the pattern: `<number>. <text>`

```
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
```

### Sorting Rules

1. Primary key: **text** (alphabetical, case-sensitive)
2. Secondary key: **number** (ascending)

Sorted output:
```
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
```

## How It Works

Two-phase algorithm:

**Phase 1 — Split & Sort** (`ChunkSorter`): Reads the input file in chunks (default 512 MB), sorts each chunk in parallel using a producer-consumer pipeline, and writes sorted chunks to temporary files.

**Phase 2 — K-Way Merge** (`KWayMerger`): Opens all sorted chunks simultaneously and merges them into the output file using a min-heap (`PriorityQueue`).

```
Input file (GB+)
    |
    v
[Phase 1: ChunkSorter]  -->  chunk_000000.tmp, chunk_000001.tmp, ...
    |
    v
[Phase 2: KWayMerger]   -->  Sorted output file
    |
    v
Cleanup temp files
```

## Projects

| Project | Description |
|---|---|
| `FileSorter` | Main sorting application |
| `FileGenerator` | Generates test files of a given size |
| `Shared` | Shared configuration (`SortConfig`) |
| `FileSorter.Tests` | Unit and integration tests (xUnit) |

## Usage

### Generate a test file

```bash
dotnet run --project src/FileGenerator -- output.txt 1GB
```

Or run without arguments for interactive mode.

### Sort a file

```bash
dotnet run --project src/FileSorter -- input.txt output.txt
```

### Options

| Flag | Description | Example |
|---|---|---|
| `--memory` | Total memory limit | `--memory 2GB` |
| `--chunk` | Chunk size in MB | `--chunk 256` |
| `--threads` | Degree of parallelism | `--threads 4` |
| `--temp` | Temporary directory | `--temp /tmp/sort` |

```bash
dotnet run --project src/FileSorter -- input.txt output.txt --memory 2GB --threads 4
```

## Run Tests

```bash
dotnet test
```

## Build

```bash
dotnet build
```
