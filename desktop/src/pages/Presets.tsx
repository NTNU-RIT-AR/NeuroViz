// Load files from the folder

import { useEffect, useState } from "react";
import { invoke } from '@tauri-apps/api/core';



async function fetchFiles(folder: String): Promise<string[]> {
  try {
    return await invoke('list_files', { folder: folder });
  } catch (error) {
    console.log("error")
  }

}

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);


  // Delete a file
  // async function deleteFile(fileName: string) {
  //   await invoke('delete_file')
  // }

  useEffect(() => {
    let folder = "/presets";
    try {
      fetchFiles(folder)
      console.log("success")
    } catch (error) {
      console.log("error")
    }

    fetchFiles(folder)

    fetchFiles(folder).then(setFiles);
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
