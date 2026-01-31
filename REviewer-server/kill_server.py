import os
import subprocess
import signal

def kill_process_on_port(port):
    print(f"Checking for processes on port {port}...")
    try:
        # Get the PID of the process using the port
        cmd = f"netstat -ano | findstr :{port}"
        output = subprocess.check_output(cmd, shell=True).decode()
        
        pids = set()
        for line in output.strip().split('\n'):
            if "LISTENING" in line:
                parts = line.split()
                if parts:
                    pids.add(parts[-1])
        
        if not pids:
            print(f"No process found on port {port}.")
            return

        for pid in pids:
            print(f"Killing process with PID {pid}...")
            try:
                subprocess.run(["taskkill", "/F", "/PID", pid], check=True)
                print(f"Successfully killed PID {pid}.")
            except subprocess.CalledProcessError:
                print(f"Failed to kill PID {pid}. It might already be closed.")

    except subprocess.CalledProcessError:
        print(f"No process listening on port {port}.")
    except Exception as e:
        print(f"Error checking port {port}: {e}")

if __name__ == "__main__":
    # Standard REviewer ports
    ports_to_kill = [8000, 5006]
    
    for port in ports_to_kill:
        kill_process_on_port(port)
        
    print("\nCleanup complete.")
