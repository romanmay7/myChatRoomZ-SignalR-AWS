import { Injectable,Output, EventEmitter } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';
import { GlobalVariable } from '../../global';

@Injectable({
  providedIn: 'root'
})
export class UploadService {

  public upload_progress: number;
  public upload_message: string;
  public attachment_name: string="";

  //AWS S3 Info
  //*************************/
  bucket:string="my-chat-roomz-images";
  region:string="s3-eu-west-1";
  //************************ */
  @Output() public onUploadFinished = new EventEmitter();
  constructor(private http: HttpClient)
  {

  }

  public uploadFile = (files) => {
    if (files.length === 0) {
      return;
    }

    let fileToUpload = <File>files[0];
    const formData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);

    //this.http.post(GlobalVariable.BASE_API_URL+"/api/UploadImage", formData, { reportProgress: true, observe: 'events' })//Regular Upload to .NET Server
    this.http.post(GlobalVariable.BASE_API_URL+"/api/UploadImageAWS", formData, { reportProgress: true, observe: 'events' }) //Upload to AWS S3
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress)
          this.upload_progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          this.upload_message = 'Attachment Success:' + fileToUpload.name; 
         // this.attachment_name = fileToUpload.name;//Regular Upload to .NET Server
          this.attachment_name = "https://"+this.bucket+"."+this.region+".amazonaws.com/"+fileToUpload.name;// Upload to AWS S3
          this.onUploadFinished.emit(event.body);
        }
      });
  }

}
