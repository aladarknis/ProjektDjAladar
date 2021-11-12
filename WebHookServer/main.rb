require 'sinatra'
require 'json'

set :bind, '0.0.0.0'

post '/payload' do
  push_json = JSON.parse(request.body.read)
  puts "I got some JSON: #{push_json.inspect}"
  ref = push_json["ref"]
  if ref.include? "main" or ref.include? "master"
    value = `#{Dir.pwd}/run_docker_compose.sh`
  end
end

