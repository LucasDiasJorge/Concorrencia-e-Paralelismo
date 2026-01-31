use std::env;
use std::thread;

const THREADS_MAX_DEFAULT: usize = 4;

fn main() {
    // Parse command line arguments
    let args: Vec<String> = env::args().collect();
    let mut threads_count = THREADS_MAX_DEFAULT;
    
    if args.len() > 1 {
        if let Ok(count) = args[1].parse::<usize>() {
            if count > 0 {
                threads_count = count;
            }
        }
    }
    
    // Limit the number of threads
    if threads_count > 64 {
        threads_count = 64;
    }

    println!("pre-execution (threads={})", threads_count);
    
    let mut handles = vec![];
    
    // Create threads
    for i in 0..threads_count {
        let thread_id = i; // Clone i for each thread
        let handle = thread::spawn(move || {
            let loops = 10;
            for j in 0..loops {
                println!("thread {}: loop {}", thread_id, j);
            }
        });
        handles.push(handle);
    }
    
    println!("mid-execution");
    
    // Wait for all threads to complete
    for handle in handles {
        handle.join().unwrap();
    }
    
    println!("post-execution");
}