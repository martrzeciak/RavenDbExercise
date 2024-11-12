'''
Author: Mariusz Trzeciak
Used resources:
https://docs.python.org/3/library/asyncio.html
ChatGPT was used to fix problem with asyncio library
'''

import asyncio
from asyncio import subprocess
import os
import sys

# Path to the C# application that is set via an environment variable
GET_TVSHOW_TOTAL_LENGTH_BIN = os.getenv("GET_TVSHOW_TOTAL_LENGTH_BIN")

if not GET_TVSHOW_TOTAL_LENGTH_BIN:
    print("Environment variable GET_TVSHOW_TOTAL_LENGTH_BIN is not set.", file=sys.stderr)
    sys.exit(1)


# Load titles from the standard input (stdin)
def load_data():
    shows = []

    # Loop through each line from the standard input (stdin)
    for line in sys.stdin:
        stripped_line = line.strip() 
        if stripped_line:
            shows.append(stripped_line)

    if not shows:
        print("No shows provided.", file=sys.stderr)
        sys.exit(1)
    
    return shows


# Function to call the C# app asynchronously and get runtime for a single show
async def get_show_runtime(show_name):
    process = await asyncio.create_subprocess_exec(
        GET_TVSHOW_TOTAL_LENGTH_BIN, show_name,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE
    )

    stdout, stderr = await process.communicate()

    if process.returncode == 0:
        try:
            return show_name, int(stdout.decode().strip())
        except ValueError:
            # Handle case where the runtime isn't a valid integer
            print(f"Invalid runtime value for {show_name}. Expected integer but got: {stdout.decode().strip()}", file=sys.stderr)
            return show_name, None
    else:
        print(f"Error for {show_name}: {stderr.decode().strip()}", file=sys.stderr)
        return show_name, None


async def main():
    show_names = load_data()
    tasks = []

    # Create a task for each show and add it to the list
    for show_name in show_names:
        tasks.append(get_show_runtime(show_name))

    results = await asyncio.gather(*tasks)

    # Filter out shows that failed to return valid runtime data
    valid_results = [(name, runtime) for name, runtime in results if runtime is not None]

    if not valid_results:
        print("No valid data retrieved.", file=sys.stderr)
        sys.exit(1)

    # Find the shortest and longest show
    shortest_show = min(valid_results, key=lambda x: x[1])
    longest_show = max(valid_results, key=lambda x: x[1])

    # Convert minutes to hours and minutes
    shortest_name, shortest_runtime = shortest_show
    longest_name, longest_runtime = longest_show

    shortest_hours, shortest_minutes = divmod(shortest_runtime, 60)
    longest_hours, longest_minutes = divmod(longest_runtime, 60)

    # Print the results
    print(f"The shortest show: {shortest_name} ({shortest_hours}h {shortest_minutes}m)")
    print(f"The longest show: {longest_name} ({longest_hours}h {longest_minutes}m)")


if __name__ == "__main__":
    asyncio.run(main())