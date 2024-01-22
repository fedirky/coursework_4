import os
import shutil

def sort_key(filename):
    parts = filename.split('_')
    return int(parts[0]), int(parts[1].split('.')[0])

def copy_files(start_index, file_count, source_dir, dest_dir):
    files = sorted(os.listdir(source_dir), key=sort_key)
    for file in files[start_index:start_index + file_count]:
        source_file = os.path.join(source_dir, file)
        dest_file = os.path.join(dest_dir, file)
        shutil.copy(source_file, dest_file)

def main():
    V = 10  # Змініть це значення відповідно до вашого варіанту
    N = 12500  # Кількість файлів у кожній з перших чотирьох директорій
    N_unsup = 50000  # Кількість файлів у п'ятій директорії

    base_dir = "datasets\\aclImdb"  # Відносний шлях до базової директорії
    dest_base_dir = "selected_files"  # Шлях до папки, де будуть зберігатися файли
    if not os.path.exists(dest_base_dir):
        os.makedirs(dest_base_dir)

    dirs = ["test\\neg", "test\\pos", "train\\neg", "train\\pos", "train\\unsup"]
    file_counts = [250, 250, 250, 250, 1000]

    for dir, file_count in zip(dirs, file_counts):
        start_index = (N // 50) * (V - 1) if dir != "train\\unsup" else (N_unsup // 50) * (V - 1)
        source_dir = os.path.join(base_dir, dir)
        dest_dir = os.path.join(dest_base_dir, dir)
        if not os.path.exists(dest_dir):
            os.makedirs(dest_dir)
        copy_files(start_index, file_count, source_dir, dest_dir)

if __name__ == "__main__":
    main()



