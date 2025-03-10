// Load files from the folder

import { BaseDirectory } from "@tauri-apps/api/path";
import { useEffect, useState } from "react";
import { invoke } from '@tauri-apps/api/core';



async function fetchFiles(): Promise<string[]> {
  return await invoke('list_files');
}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);


  // Delete a file
  // async function deleteFile(fileName: string) {
  //   await invoke('delete_file')
  // }

  useEffect(() => {
    fetchFiles().then(setFiles);
  }, [])

  return (
    <>
      <p>presets page text</p>
      <ul>
        {files.map((file) => (
          <li key={file}>{file}</li>
        ))}
      </ul>
    </>

  )
}
