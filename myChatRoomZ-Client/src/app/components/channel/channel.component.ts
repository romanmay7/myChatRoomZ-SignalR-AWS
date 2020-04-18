import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from '@angular/router';
import * as Signal_R from '@aspnet/signalr';
import { Message } from '../../data-models/message.model';
import { ChannelService } from '../../services/channel.service';
import { UploadService } from '../../services/upload.service';
import { Channel } from '../../data-models/channel.model';
import { trigger, style, transition, animate, keyframes, query, stagger, state } from "@angular/animations";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { LoginService } from '../../services/login.service';
import { GlobalVariable } from '../../../global';

@Component({
  selector: 'app-channel',
  templateUrl: './channel.component.html',
  styleUrls: ['./channel.component.css'],
  animations: [
    trigger('message_history', [
      state('in', style({ transform: 'translateX(0)' })),
      transition('void => *', [
        animate('.6s ease-in', keyframes([
          style({ opacity: 0, transform: 'translateY(-75%)', offset: 0 }),
          style({ opacity: .5, transform: 'translateY(35px)', offset: .3 }),
          style({ opacity: 1, transform: 'translateY(0)', offset: 1 }),
        ]))
      ]),
      transition('* => void', [
        animate('.6s ease-in', keyframes([
          style({ opacity: 1, transform: 'translateY(0)', offset: 0 }),
          style({ opacity: .5, transform: 'translateY(35px)', offset: .3 }),
          style({ opacity: 0, transform: 'translateY(-75%)', offset: 1 }),
        ]))
      ])
    ])
  ]
})
export class ChannelComponent implements OnInit {

  public channel_id: string;
  public current_channel: Channel;

  public connection: any;//SignalR Hub Client's Connection Object
  public isConnected:boolean=false 
  public new_message: Message;
  public message_history: Message[] = [];
  chatForm: FormGroup;

 

  constructor(private formBuilder: FormBuilder, public channelService: ChannelService, public uploadService: UploadService,
    private activatedRoute: ActivatedRoute, private http: HttpClient, public loginService: LoginService, private router: Router) { }

  async ngOnInit() {

    if (this.loginService.loginRequired) {
      //Force Login
      this.router.navigate(["login"])
    } 

    //Track changes of id parameter in Route
    this.activatedRoute.params.subscribe(async params => {
      this.channel_id = params['id'];
 
      //Request  Channel's data by ID from the Service
      if (this.channel_id) { this.current_channel = await this.channelService.find(this.channel_id) };

      this.message_history = this.current_channel.messageHistory//in case the Channel already have Message History
      //If Connection Object already exists, invoke JoinChannel function on the Hub,passing to it a Name of the Client and Channel ID
      if (this.isConnected) { console.log(this.connection + ":connected to hub already"); this.connection.invoke('JoinChannel', this.channelService.chatterName, this.channel_id); }
    });


    this.chatForm = this.formBuilder.group({ msgText: [] });//Build Form Object for Chat Messages

    // Initialize the SignalR Client
    //******************************************************************************************************
    //1.Creating Connection Object
    if (!this.isConnected) {
      this.connection = new Signal_R.HubConnectionBuilder()
        .withUrl(GlobalVariable.BASE_API_URL+'/chatHub')
        .build();
      //2.Initialize Connection
      this.connection
        .start()
        //When new Client Connects to the Hub we will invoke JoinChannel function on the Hub,passing to it a Name of the Client and Channel ID
        .then(() => { this.isConnected = true; this.connection.invoke('JoinChannel', this.channelService.chatterName, this.channel_id); })
        .catch(err => console.log('Error while starting connection: ' + err))

      //3.Listening to 'JoinChannel' Request from the Hub.
      //When it invoked, we will recieve the Names of Newly Connected Clients and add them to 'connected_clients' array
      this.connection.on('JoinChannel', (newClient: string,ch_id:string) => {

        if (this.channelService.connected_clients[parseInt(ch_id)].indexOf(newClient) === -1)
          this.channelService.connected_clients[parseInt(ch_id)].push(newClient);
      });

      //4.Listening to 'LeaveChannel' Request from the Hub.
      //When it invoked, we will recieve the Names of Leaving Clients and remove them from 'connected_clients' array
      this.connection.on('LeaveChannel', (connectedClient: string, ch_id: string) => {
        console.log("The client " + connectedClient + " is leaving channel " + ch_id);

         //Remove leaving Chatter from the Array of 'connected_clients' for Specific Channel
        var index = this.channelService.connected_clients[parseInt(ch_id)].indexOf(connectedClient) 
        if (index !== -1)
         this.channelService.connected_clients[parseInt(ch_id)].splice(index, 1)
        //this.channelService.connected_clients[parseInt(ch_id)] = this.channelService.connected_clients[parseInt(ch_id)].filter(e => e !== connectedClient); 
      });

      //5.On Recieving Message from Clients who calls 'SendMessage on the Hub'
      this.connection.on('RecieveMessage', (msg: Message,id:number) => {
        //this.new_message = msg;
        this.message_history.push(msg);

        //in the case the Sender is not included in the list of connected clients for the Specific Channel,Add him to the list
        if (this.channelService.connected_clients[id].indexOf(msg.senderName) === -1)
          this.channelService.connected_clients[id].push(msg.senderName);
      });

      //6.On Recieving RemoveMessage from  the SignalRHub'
      this.connection.on('RemoveMessage', (date: Date) => {
        var index;
        for (var i = 0; i < this.message_history.length; i++)
        {
          if (date == this.message_history[i].sentAt)
            index = i;
        }
        this.message_history.splice(index, 1)
      });
    }
  }
  //**************************************************************************************************************
  onRemove(msg: Message)
   {
    if (msg.senderName == this.channelService.chatterName) {
      //Delete Message from the Server
      this.http.post(GlobalVariable.BASE_API_URL+"/api/DeleteMessage", msg,
        {
          headers: new HttpHeaders()
            .set('Content-Type', 'application/json; charset=utf-8')
          //.set(''Content-Type',application/x-www-form-urlencoded')
        })
        .subscribe(
          (response) => {
            console.log("Deleted Message:" + response);
            // invoke 'RemoveMessage' on the SignalR Hub to Update UI for all Connected Clients
            this.connection.invoke('RemoveMessage', msg.sentAt, this.channel_id);
          },
          (error) => { alert("Could not remove the message"); console.log(error); }
        )
    }
    else { alert("Only The Actual Poster of the Message or Admin are allowed to Delete it");}
  }
  //-----------------------------------------------------------------------------------------------------------------
  onSubmit() {
    this.sendMessage(this.chatForm.get('msgText').value);
  }
  
  sendMessage(chatform_message)
  {
    console.log(this.chatForm.get('msgText').value);
    //
    var new_message = new Message();
    new_message.text = this.chatForm.get('msgText').value;
    new_message.senderName = this.channelService.chatterName;
    new_message.channelId = parseInt(this.channel_id);
    new_message.sentAt = new Date();
    if (this.uploadService.attachment_name!="") { new_message.attachment = this.uploadService.attachment_name; }

    //Post Message to the Server
    this.http.post(GlobalVariable.BASE_API_URL+"/api/PostMessage", new_message,
      {
        headers: new HttpHeaders()
          .set('Content-Type', 'application/json; charset=utf-8')
        //.set(''Content-Type',application/x-www-form-urlencoded')
      })
      .subscribe(
        (response) =>
        {
          console.log("Posted Message:" + response);

          // invoke 'SendMessage' on the SignalR Hub to Update UI for all Connected Clients
          this.connection.invoke('SendMessage', this.channelService.chatterName, chatform_message, this.channel_id, response["id"], this.uploadService.attachment_name);
          //Reset Attachment
          this.uploadService.attachment_name = "";
          this.uploadService.upload_message = "";
          this.uploadService.upload_progress = 0;
        },
        (error) => { alert("Could not post a message"); console.log(error); }
      )
  }
  //--------------------------------------------------------------------------------------------------------------------------------------
 //For Upload to .NET Server
  createImgPath(img_name: string): string
  {
    return GlobalVariable.BASE_API_URL+"/Resources/Images/" + img_name;
  }

}

